using KeePass.Plugins;
using KeePass.UI;
using KeePass.Util.Spr;
using KeePassHttp.Protocol;
using KeePassHttp.Protocol.Action;
using KeePassLib;
using KeePassLib.Collections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace KeePassHttp.Entry
{
    public sealed class EntrySearch
    {
        private IPluginHost _host;
        private KeePassHttpExt _ext;

        public EntrySearch()
        {
            _host = KeePassHttpExt.HostInstance;
            _ext = KeePassHttpExt.ExtInstance;
        }

        internal Response GetLoginsHandler(Request req)
        {
            if (!req.TryDecrypt()) return new ErrorResponse(req.Action, ErrorType.CannotDecryptMessage);

            var msg = req.Message;
            var id = msg.GetString("id");
            var url = msg.GetString("url");
            var submitUrl = msg.GetString("submitUrl");

            Uri hostUri;
            Uri submitUri;

            if (!string.IsNullOrEmpty(url))
            {
                hostUri = new Uri(url);
            }
            else
            {
                return new ErrorResponse(req.Action, ErrorType.NoUrlProvided);
            }

            if (!string.IsNullOrEmpty(submitUrl))
            {
                submitUri = new Uri(submitUrl);
            }
            else
            {
                submitUri = hostUri;
            }

            var resp = req.GetResponse();
            resp.Message.Add("id", id);

            var items = FindMatchingEntries(url, submitUrl, null);
            if (items.ToList().Count > 0)
            {
                Func<PwEntry, bool> filter = delegate (PwEntry e)
                {
                    var c = _ext.GetEntryConfig(e);

                    var title = e.Strings.ReadSafe(PwDefs.TitleField);
                    var entryUrl = e.Strings.ReadSafe(PwDefs.UrlField);
                    if (c != null)
                    {
                        return title != hostUri.Host && entryUrl != hostUri.Host && !c.Allow.Contains(hostUri.Host) || (submitUri.Host != null && !c.Allow.Contains(submitUri.Host) && submitUri.Host != title && submitUri.Host != entryUrl);
                    }
                    return title != hostUri.Host && entryUrl != hostUri.Host || (submitUri.Host != null && title != submitUri.Host && entryUrl != submitUri.Host);
                };

                var configOpt = new ConfigOpt(_host.CustomConfig);
                var config = _ext.GetConfigEntry(true);
                var autoAllowS = config.Strings.ReadSafe("Auto Allow");
                var autoAllow = autoAllowS != null && autoAllowS.Trim() != "";
                autoAllow = autoAllow || configOpt.AlwaysAllowAccess;
                var needPrompting = from e in items where filter(e.entry) select e;

                if (needPrompting.ToList().Count > 0 && !autoAllow)
                {
                    var win = _host.MainWindow;

                    using (var f = new AccessControlForm())
                    {
                        win.Invoke((MethodInvoker)delegate
                        {
                            f.Icon = win.Icon;
                            f.Plugin = _ext;
                            f.Entries = (from e in items where filter(e.entry) select e.entry).ToList();
                            //f.Entries = needPrompting.ToList();
                            f.Host = submitUri.Host != null ? submitUri.Host : hostUri.Host;
                            f.Load += delegate { f.Activate(); };
                            f.ShowDialog(win);
                            if (f.Remember && (f.Allowed || f.Denied))
                            {
                                foreach (var e in needPrompting)
                                {
                                    var c = _ext.GetEntryConfig(e.entry);
                                    if (c == null)
                                        c = new EntryConfig();
                                    var set = f.Allowed ? c.Allow : c.Deny;
                                    set.Add(hostUri.Host);
                                    if (submitUri.Host != null && submitUri.Host != hostUri.Host)
                                        set.Add(submitUri.Host);
                                    _ext.SetEntryConfig(e.entry, c);

                                }
                            }
                            if (!f.Allowed)
                            {
                                items = items.Except(needPrompting);
                            }
                        });
                    }
                }

                foreach (var entryDatabase in items)
                {
                    string entryUrl = String.Copy(entryDatabase.entry.Strings.ReadSafe(PwDefs.UrlField));
                    if (String.IsNullOrEmpty(entryUrl))
                        entryUrl = entryDatabase.entry.Strings.ReadSafe(PwDefs.TitleField);

                    entryUrl = entryUrl.ToLower();

                    entryDatabase.entry.UsageCount = (ulong)LevenshteinDistance(submitUrl.ToLower(), entryUrl);

                }

                var itemsList = items.ToList();

                if (configOpt.SpecificMatchingOnly)
                {
                    itemsList = (from e in itemsList
                                 orderby e.entry.UsageCount ascending
                                 select e).ToList();

                    ulong lowestDistance = itemsList.Count > 0 ?
                        itemsList[0].entry.UsageCount :
                        0;

                    itemsList = (from e in itemsList
                                 where e.entry.UsageCount == lowestDistance
                                 orderby e.entry.UsageCount
                                 select e).ToList();

                }

                if (configOpt.SortResultByUsername)
                {
                    var items2 = from e in itemsList orderby e.entry.UsageCount ascending, _ext.GetUserPass(e)[0] ascending select e;
                    itemsList = items2.ToList();
                }
                else
                {
                    var items2 = from e in itemsList orderby e.entry.UsageCount ascending, e.entry.Strings.ReadSafe(PwDefs.TitleField) ascending select e;
                    itemsList = items2.ToList();
                }

                var entries = new JArray(itemsList.Select(item =>
                {
                    var up = _ext.GetUserPass(item);
                    JArray fldArr = null;
                    var fields = GetFields(configOpt, item);
                    if (fields != null)
                    {
                        fldArr = new JArray(fields.Select(f => new JObject { f.Key, f.Value }));
                    }
                    return new JObject {
                        { "name", item.entry.Strings.ReadSafe(PwDefs.TitleField) },
                        { "login", up[0] },
                        { "password", up[1] },
                        { "uuid", item.entry.Uuid.ToHexString() },
                        { "fields", fldArr }
                    };
                }));

                resp.Message.Add("count", itemsList.Count);
                resp.Message.Add("entries", entries);

                if (itemsList.Count > 0)
                {
                    var names = (from e in itemsList select e.entry.Strings.ReadSafe(PwDefs.TitleField)).Distinct();
                    var n = String.Join("\n    ", names);

                    if (configOpt.ReceiveCredentialNotification)
                        _ext.ShowNotification(String.Format("{0}: {1} is receiving credentials for:\n    {2}", req.GetString("id"), hostUri.Host, n));
                }

                return resp;
            }

            resp.Message.Add("count", 0);
            resp.Message.Add("entries", new JArray());

            return resp;
        }

        //http://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Levenshtein_distance#C.23
        private int LevenshteinDistance(string source, string target)
        {
            if (String.IsNullOrEmpty(source))
            {
                if (String.IsNullOrEmpty(target)) return 0;
                return target.Length;
            }
            if (String.IsNullOrEmpty(target)) return source.Length;

            if (source.Length > target.Length)
            {
                var temp = target;
                target = source;
                source = temp;
            }

            var m = target.Length;
            var n = source.Length;
            var distance = new int[2, m + 1];
            // Initialize the distance 'matrix'
            for (var j = 1; j <= m; j++) distance[0, j] = j;

            var currentRow = 0;
            for (var i = 1; i <= n; ++i)
            {
                currentRow = i & 1;
                distance[currentRow, 0] = i;
                var previousRow = currentRow ^ 1;
                for (var j = 1; j <= m; j++)
                {
                    var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
                    distance[currentRow, j] = Math.Min(Math.Min(
                                            distance[previousRow, j] + 1,
                                            distance[currentRow, j - 1] + 1),
                                            distance[previousRow, j - 1] + cost);
                }
            }
            return distance[currentRow, m];
        }

        private IEnumerable<KeyValuePair<string, string>> GetFields(ConfigOpt configOpt, PwEntryDatabase entryDatabase)
        {
            SprContext ctx = new SprContext(entryDatabase.entry, entryDatabase.database, SprCompileFlags.All, false, false);

            List<KeyValuePair<string, string>> fields = null;
            if (configOpt.ReturnStringFields)
            {
                fields = new List<KeyValuePair<string, string>>();
                foreach (var sf in entryDatabase.entry.Strings)
                {
                    var sfValue = entryDatabase.entry.Strings.ReadSafe(sf.Key);

                    // follow references
                    sfValue = SprEngine.Compile(sfValue, ctx);

                    if (configOpt.ReturnStringFieldsWithKphOnly)
                    {
                        if (sf.Key.StartsWith("KPH: "))
                        {
                            fields.Add(new KeyValuePair<string, string>(sf.Key.Substring(5), sfValue));
                        }
                    }
                    else
                    {
                        fields.Add(new KeyValuePair<string, string>(sf.Key, sfValue));
                    }
                }

                if (fields.Count > 0)
                {
                    var sorted = from e2 in fields orderby e2.Key ascending select e2;
                    fields = sorted.ToList();
                }
                else
                {
                    fields = null;
                }
            }

            return fields;
        }

        private IEnumerable<PwEntryDatabase> FindMatchingEntries(string url, string submitUrl, string realm)
        {
            string submitHost = null;
            var listResult = new List<PwEntryDatabase>();
            var hostUri = new Uri(url);

            var formHost = hostUri.Host;
            var searchHost = hostUri.Host;
            var origSearchHost = hostUri.Host;
            var parms = MakeSearchParameters();

            List<PwDatabase> listDatabases = new List<PwDatabase>();

            var configOpt = new ConfigOpt(_host.CustomConfig);
            if (configOpt.SearchInAllOpenedDatabases)
            {
                foreach (PwDocument doc in _host.MainWindow.DocumentManager.Documents)
                {
                    if (doc.Database.IsOpen)
                    {
                        listDatabases.Add(doc.Database);
                    }
                }
            }
            else
            {
                listDatabases.Add(_host.Database);
            }

            int listCount = 0;
            foreach (PwDatabase db in listDatabases)
            {
                searchHost = origSearchHost;
                //get all possible entries for given host-name
                while (listResult.Count == listCount && (origSearchHost == searchHost || searchHost.IndexOf(".") != -1))
                {
                    parms.SearchString = String.Format("^{0}$|/{0}/?", searchHost);
                    var listEntries = new PwObjectList<PwEntry>();
                    db.RootGroup.SearchEntries(parms, listEntries);
                    foreach (var le in listEntries)
                    {
                        listResult.Add(new PwEntryDatabase(le, db));
                    }
                    searchHost = searchHost.Substring(searchHost.IndexOf(".") + 1);

                    //searchHost contains no dot --> prevent possible infinite loop
                    if (searchHost == origSearchHost)
                        break;
                }
                listCount = listResult.Count;
            }


            Func<PwEntry, bool> filter = delegate (PwEntry e)
            {
                var title = e.Strings.ReadSafe(PwDefs.TitleField);
                var entryUrl = e.Strings.ReadSafe(PwDefs.UrlField);
                var c = _ext.GetEntryConfig(e);
                if (c != null)
                {
                    if (c.Allow.Contains(formHost) && (submitHost == null || c.Allow.Contains(submitHost)))
                        return true;
                    if (c.Deny.Contains(formHost) || (submitHost != null && c.Deny.Contains(submitHost)))
                        return false;
                    if (realm != null && c.Realm != realm)
                        return false;
                }

                if (entryUrl != null && (entryUrl.StartsWith("http://") || entryUrl.StartsWith("https://") || title.StartsWith("ftp://") || title.StartsWith("sftp://")))
                {
                    var uHost = new Uri(entryUrl);
                    if (formHost.EndsWith(uHost.Host))
                        return true;
                }

                if (title.StartsWith("http://") || title.StartsWith("https://") || title.StartsWith("ftp://") || title.StartsWith("sftp://"))
                {
                    var uHost = new Uri(title);
                    if (formHost.EndsWith(uHost.Host))
                        return true;
                }
                return formHost.Contains(title) || (entryUrl != null && formHost.Contains(entryUrl));
            };

            Func<PwEntry, bool> filterSchemes = delegate (PwEntry e)
            {
                var title = e.Strings.ReadSafe(PwDefs.TitleField);
                var entryUrl = e.Strings.ReadSafe(PwDefs.UrlField);

                if (entryUrl != null)
                {
                    var entryUri = new Uri(entryUrl);
                    if (entryUri.Scheme == hostUri.Scheme)
                    {
                        return true;
                    }
                }

                var titleUri = new Uri(title);
                if (titleUri.Scheme == hostUri.Scheme)
                {
                    return true;
                }

                return false;
            };

            var result = from e in listResult where filter(e.entry) select e;

            if (configOpt.MatchSchemes)
            {
                result = from e in result where filterSchemes(e.entry) select e;
            }

            Func<PwEntry, bool> hideExpired = delegate (PwEntry e)
            {
                DateTime dtNow = DateTime.UtcNow;

                if (e.Expires && (e.ExpiryTime <= dtNow))
                {
                    return false;
                }

                return true;
            };

            if (configOpt.HideExpired)
            {
                result = from e in result where hideExpired(e.entry) select e;
            }

            return result;
        }

        private SearchParameters MakeSearchParameters()
        {
            return new SearchParameters
            {
                SearchInTitles = true,
                RegularExpression = true,
                SearchInGroupNames = false,
                SearchInNotes = false,
                SearchInOther = false,
                SearchInPasswords = false,
                SearchInTags = false,
                SearchInUrls = true,
                SearchInUserNames = false,
                SearchInUuids = false
            };
        }
    }
}
