using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Windows.Forms;
using System.Security.Cryptography;

using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Security;

using Newtonsoft.Json;
using KeePass.Util.Spr;
using KeePassHttp.Protocol;
using System.Collections.Generic;
using KeePassHttp.Protocol.Action;
using KeePassLib.Collections;

namespace KeePassHttp
{
    public sealed class KeePassHttpExt : Plugin
    {

        /// <summary>
        /// an arbitrarily generated uuid for the keepasshttp root entry
        /// </summary>
        public readonly byte[] KEEPASSHTTP_UUID = {
                0x34, 0x69, 0x7a, 0x40, 0x8a, 0x5b, 0x41, 0xc0,
                0x9f, 0x36, 0x89, 0x7d, 0x62, 0x3e, 0xcb, 0x31
                                                };

        private const int DEFAULT_NOTIFICATION_TIME = 5000;
        public const string KEEPASSHTTP_NAME = "KeePassHttp2 Settings";
        private const string KEEPASSHTTP_GROUP_NAME = "KeePassHttp Passwords";
        // internal const string KEEPASSHTTP_KEYPAIR_NAME = "Key Pair";
        public const string ASSOCIATE_KEY_PREFIX = "Public Key: ";
        internal IPluginHost host;
        private HttpListener listener;
        public const int DEFAULT_PORT = 19455;
        public const string DEFAULT_HOST = "localhost";
        /// <summary>
        /// TODO make configurable
        /// </summary>
        private const string HTTP_SCHEME = "http://";
        //private const string HTTPS_PREFIX = "https://localhost:";
        //private int HTTPS_PORT = DEFAULT_PORT + 1;
        private Thread httpThread;
        private volatile bool stopped = false;
        // Dictionary<string, RequestHandler> handlers = new Dictionary<string, RequestHandler>();

        //public string UpdateUrl = "";
        public override string UpdateUrl { get { return "https://passifox.appspot.com/kph/latest-version.txt"; } }

        private Handlers _handlers;

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

        internal PwEntry GetConfigEntry(bool create)
        {
            var root = host.Database.RootGroup;
            var uuid = new PwUuid(KEEPASSHTTP_UUID);
            var entry = root.FindEntry(uuid, false);
            if (entry == null && create)
            {
                entry = new PwEntry(false, true);
                entry.Uuid = uuid;
                entry.Strings.Set(PwDefs.TitleField, new ProtectedString(false, KEEPASSHTTP_NAME));
                root.AddEntry(entry, true);
                UpdateUI(null);
            }
            return entry;
        }

        private int GetNotificationTime()
        {
            var time = DEFAULT_NOTIFICATION_TIME;
            var entry = GetConfigEntry(false);
            if (entry != null)
            {
                var s = entry.Strings.ReadSafe("Prompt Timeout");
                if (s != null && s.Trim() != "")
                {
                    try
                    {
                        time = Int32.Parse(s) * 1000;
                    }
                    catch { }
                }
            }

            return time;
        }

        internal void ShowNotification(string text)
        {
            ShowNotification(text, null, null);
        }

        private void ShowNotification(string text, EventHandler onclick)
        {
            ShowNotification(text, onclick, null);
        }

        private void ShowNotification(string text, EventHandler onclick, EventHandler onclose)
        {
            MethodInvoker m = delegate
            {
                var notify = host.MainWindow.MainNotifyIcon;
                if (notify == null)
                    return;

                EventHandler clicked = null;
                EventHandler closed = null;

                clicked = delegate
                {
                    notify.BalloonTipClicked -= clicked;
                    notify.BalloonTipClosed -= closed;
                    if (onclick != null)
                        onclick(notify, null);
                };
                closed = delegate
                {
                    notify.BalloonTipClicked -= clicked;
                    notify.BalloonTipClosed -= closed;
                    if (onclose != null)
                        onclose(notify, null);
                };

                //notify.BalloonTipIcon = ToolTipIcon.Info;
                notify.BalloonTipTitle = "KeePassHttp";
                notify.BalloonTipText = text;
                notify.ShowBalloonTip(GetNotificationTime());
                // need to add listeners after showing, or closed is sent right away
                notify.BalloonTipClosed += closed;
                notify.BalloonTipClicked += clicked;
            };
            if (host.MainWindow.InvokeRequired)
                host.MainWindow.Invoke(m);
            else
                m.Invoke();
        }

        public override bool Initialize(IPluginHost host)
        {
            var httpSupported = HttpListener.IsSupported;
            this.host = host;

            var optionsMenu = new ToolStripMenuItem("KeePassHttp Options...");
            optionsMenu.Click += OnOptions_Click;
            optionsMenu.Image = KeePassHttp.Properties.Resources.earth_lock;
            //optionsMenu.Image = global::KeePass.Properties.Resources.B16x16_File_Close;
            this.host.MainWindow.ToolsMenu.DropDownItems.Add(optionsMenu);

            if (httpSupported)
            {
                try
                {
                    _handlers = new Protocol.Handlers(this);
                    _handlers.Initialize();

                    /*
                    handlers.Add(Request.TEST_ASSOCIATE, TestAssociateHandler);
                    handlers.Add(Request.ASSOCIATE, AssociateHandler);
                    handlers.Add(Request.GET_LOGINS, GetLoginsHandler);
                    handlers.Add(Request.GET_LOGINS_COUNT, GetLoginsCountHandler);
                    handlers.Add(Request.GET_ALL_LOGINS, GetAllLoginsHandler);
                    handlers.Add(Request.SET_LOGIN, SetLoginHandler);
                    handlers.Add(Request.GENERATE_PASSWORD, GeneratePassword);
                    */

                    listener = new HttpListener();

                    var configOpt = new ConfigOpt(this.host.CustomConfig);

                    listener.Prefixes.Add(HTTP_SCHEME + configOpt.ListenerHost + ":" + configOpt.ListenerPort.ToString() + "/");
                    //listener.Prefixes.Add(HTTPS_PREFIX + HTTPS_PORT + "/");
                    listener.Start();

                    httpThread = new Thread(new ThreadStart(Run));
                    httpThread.Start();
                } catch (HttpListenerException e) {
                    MessageBox.Show(host.MainWindow,
                        "Unable to start HttpListener!\nDo you really have only one installation of KeePassHttp in your KeePass-directory?\n\n" + e,
                        "Unable to start HttpListener",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            else
            {
                MessageBox.Show(host.MainWindow, "The .NET HttpListener is not supported on your OS",
                        ".NET HttpListener not supported",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
            }
            return httpSupported;
        }

        void OnOptions_Click(object sender, EventArgs e)
        {
            var form = new OptionsForm(new ConfigOpt(host.CustomConfig));
            UIUtil.ShowDialogAndDestroy(form);
        }

        private void Run()
        {
            while (!stopped)
            {
                try
                {
                    var r = listener.BeginGetContext(new AsyncCallback(RequestHandler), listener);
                    r.AsyncWaitHandle.WaitOne();
                    r.AsyncWaitHandle.Close();
                }
                catch (ThreadInterruptedException) { }
                catch (HttpListenerException e) {
                    MessageBox.Show(host.MainWindow, "Unable to process request!\n\n" + e,
                        "Unable to process request",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private JsonSerializer NewJsonSerializer()
        {
            var settings = new JsonSerializerSettings();
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            settings.NullValueHandling = NullValueHandling.Ignore;

            return JsonSerializer.Create(settings);
        }

        private Response ProcessRequest(Request req, HttpListenerResponse resp)
        {
            var handler = _handlers.GetHandler(req.Action);
            if (handler != null)
            {
                try
                {
                    return handler.Invoke(req);
                }
                catch (Exception ex)
                {
                    ShowNotification("***BUG*** " + ex, (s, evt) => MessageBox.Show(host.MainWindow, ex.ToString()));
                    // response.error = ex.ToString();
                    resp.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            return null;
        }

        private void RequestHandler(IAsyncResult r) 
        {
            try {
                _RequestHandler(r);
            } catch (Exception e) {
                MessageBox.Show(host.MainWindow, "RequestHandler failed: " + e);
            }
        }

        private void _RequestHandler(IAsyncResult r)
        {
            if (stopped) return;
            var l    = (HttpListener)r.AsyncState;
            var ctx  = l.EndGetContext(r);
            var req  = ctx.Request;
            var resp = ctx.Response;

            var serializer = NewJsonSerializer();

            resp.StatusCode = (int)HttpStatusCode.OK;

            Request request = null;

            try
            {
                request = Request.ReadFromStream(req.InputStream);
            }
            catch (JsonException e)
            {
                var buffer = Encoding.UTF8.GetBytes(e + "");
                resp.StatusCode = (int)HttpStatusCode.BadRequest;
                resp.ContentLength64 = buffer.Length;
                resp.OutputStream.Write(buffer, 0, buffer.Length);
            }

            var db = host.Database;

            var configOpt = new ConfigOpt(this.host.CustomConfig);

            if (request != null && (configOpt.UnlockDatabaseRequest) && !db.IsOpen)
            {
                host.MainWindow.Invoke((MethodInvoker)delegate
                {
                    host.MainWindow.EnsureVisibleForegroundWindow(true, true);
                });

                // UnlockDialog not already opened
                bool bNoDialogOpened = (KeePass.UI.GlobalWindowManager.WindowCount == 0);
                if (!db.IsOpen && bNoDialogOpened)
                {
                    host.MainWindow.Invoke((MethodInvoker)delegate
                    {
                        host.MainWindow.OpenDatabase(host.MainWindow.DocumentManager.ActiveDocument.LockedIoc, null, false);
                    });
                }
            }

            if (request != null && db.IsOpen)
            {
                Response response = null;
                if (request != null)
                    response = ProcessRequest(request, resp);

                resp.ContentType = "application/json";
                var writer = new StringWriter();
                if (response != null)
                {
                    serializer.Serialize(writer, response);
                    var buffer = Encoding.UTF8.GetBytes(writer.ToString());
                    resp.ContentLength64 = buffer.Length;
                    resp.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }
            else
            {
                resp.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            }

            var outs = resp.OutputStream;
            outs.Close();
            resp.Close();
        }

        public override void Terminate()
        {
            stopped = true;
            listener.Stop();
            listener.Close();
            httpThread.Interrupt();
        }

        internal void UpdateUI(PwGroup group)
        {
            var win = host.MainWindow;
            if (group == null) group = host.Database.RootGroup;
            var f = (MethodInvoker) delegate {
                win.UpdateUI(false, null, true, group, true, null, true);
            };
            if (win.InvokeRequired)
                win.Invoke(f);
            else
                f.Invoke();
        }

        internal string[] GetUserPass(PwEntry entry)
        {
            return GetUserPass(new PwEntryDatabase(entry, host.Database));
        }

        internal string[] GetUserPass(PwEntryDatabase entryDatabase)
        {
            // follow references
            SprContext ctx = new SprContext(entryDatabase.entry, entryDatabase.database,
                    SprCompileFlags.All, false, false);

            return GetUserPass(entryDatabase, ctx);
        }

        internal string[] GetUserPass(PwEntryDatabase entryDatabase, SprContext ctx)
        {
            // follow references
            string user = SprEngine.Compile(
                    entryDatabase.entry.Strings.ReadSafe(PwDefs.UserNameField), ctx);
            string pass = SprEngine.Compile(
                    entryDatabase.entry.Strings.ReadSafe(PwDefs.PasswordField), ctx);
            var f = (MethodInvoker)delegate
            {
                // apparently, SprEngine.Compile might modify the database
                host.MainWindow.UpdateUI(false, null, false, null, false, null, false);
            };
            if (host.MainWindow.InvokeRequired)
                host.MainWindow.Invoke(f);
            else
                f.Invoke();

            return new string[] { user, pass };
        }

        internal string GetDbHash()
        {
            var ms = new MemoryStream();
            ms.Write(host.Database.RootGroup.Uuid.UuidBytes, 0, 16);
            ms.Write(host.Database.RecycleBinUuid.UuidBytes, 0, 16);
            var sha256 = new SHA256CryptoServiceProvider();
            var hashBytes = sha256.ComputeHash(ms.ToArray());
            return ByteToHexBitFiddle(hashBytes);
        }

        // wizard magic courtesy of https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/14333437#14333437
        static string ByteToHexBitFiddle(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }

        internal string ShowConfirmAssociationDialog(string key)
        {
            string id = null;
            using (var f = new ConfirmAssociationForm())
            {
                var win = host.MainWindow;
                win.Invoke((MethodInvoker)delegate
                {
                    f.Activate();
                    f.Icon = win.Icon;
                    f.Key = key;
                    f.Load += delegate { f.Activate(); };
                    f.ShowDialog(win);

                    if (f.KeyId != null)
                    {
                        var entry = GetConfigEntry(true);

                        bool keyNameExists = true;
                        while (keyNameExists)
                        {
                            DialogResult keyExistsResult = DialogResult.Yes;
                            if (entry.Strings.Any(x => x.Key == ASSOCIATE_KEY_PREFIX + f.KeyId))
                            {
                                keyExistsResult = MessageBox.Show(
                                    win,
                                    "A shared encryption-key with the name \"" + f.KeyId + "\" already exists.\nDo you want to overwrite it?",
                                    "Overwrite existing key?",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning,
                                    MessageBoxDefaultButton.Button1
                                );
                            }

                            if (keyExistsResult == DialogResult.No)
                            {
                                f.ShowDialog(win);
                            }
                            else
                            {
                                keyNameExists = false;
                            }
                        }

                        if (f.KeyId != null)
                        {
                            entry.Strings.Set(ASSOCIATE_KEY_PREFIX + f.KeyId, new ProtectedString(true, key));
                            entry.Touch(true);
                            UpdateUI(null);
                            id = f.KeyId;
                        }
                    }
                });
            }
            return id;
        }

        internal void ShowAccessControlDialog(string submitUrl, IList<PwEntryDatabase> items)
        {
            var win = host.MainWindow;

            using (var f = new AccessControlForm())
            {
                win.Invoke((MethodInvoker)delegate
                {
                    f.Icon = win.Icon;
                    f.Plugin = this;
                    f.Entries = items.Select(item => item.entry).ToList();
                    f.Host = submitUrl;
                    f.Load += delegate { f.Activate(); };
                    f.ShowDialog(win);
                    if (f.Remember && (f.Allowed || f.Denied))
                    {
                        foreach (var e in items)
                        {
                            var c = GetEntryConfig(e.entry);
                            if (c == null)
                                c = new EntryConfig();
                            var set = f.Allowed ? c.Allow : c.Deny;
                            set.Add(submitUrl);
                            /*
                            if (submithost != null && submithost != host)
                                set.Add(submithost);
                                */
                            SetEntryConfig(e.entry, c);
                        }
                    }
                    if (!f.Allowed)
                    {
                        // items = items.Except(needPrompting);
                    }
                });
            }
        }

        internal EntryConfig GetEntryConfig(PwEntry e)
        {
            var serializer = NewJsonSerializer();
            if (e.Strings.Exists(KEEPASSHTTP_NAME))
            {
                var json = e.Strings.ReadSafe(KEEPASSHTTP_NAME);
                using (var ins = new JsonTextReader(new StringReader(json)))
                {
                    return serializer.Deserialize<EntryConfig>(ins);
                }
            }
            return null;
        }

        internal void SetEntryConfig(PwEntry e, EntryConfig c)
        {
            var serializer = NewJsonSerializer();
            var writer = new StringWriter();
            serializer.Serialize(writer, c);
            e.Strings.Set(KEEPASSHTTP_NAME, new ProtectedString(false, writer.ToString()));
            e.Touch(true);
            UpdateUI(e.ParentGroup);
        }

        internal IEnumerable<PwEntryDatabase> FindMatchingEntries(string url, string submitUrl, string realm)
        {
            string submitHost = null;
            var listResult = new List<PwEntryDatabase>();
            var hostUri = new Uri(url);

            var formHost = hostUri.Host;
            var searchHost = hostUri.Host;
            var origSearchHost = hostUri.Host;
            var parms = MakeSearchParameters();

            List<PwDatabase> listDatabases = new List<PwDatabase>();

            var configOpt = new ConfigOpt(this.host.CustomConfig);
            if (configOpt.SearchInAllOpenedDatabases)
            {
                foreach (PwDocument doc in host.MainWindow.DocumentManager.Documents)
                {
                    if (doc.Database.IsOpen)
                    {
                        listDatabases.Add(doc.Database);
                    }
                }
            }
            else
            {
                listDatabases.Add(host.Database);
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
                var c = GetEntryConfig(e);
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
    }
}
