using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Security;
using KeePassLib.Utility;
using System;
using System.IO;
using System.Windows.Forms;

namespace KeePassNatMsg.Entry
{
    public sealed class EntryUpdate
    {
        private IPluginHost _host;
        private KeePassNatMsgExt _ext;

        public EntryUpdate()
        {
            _host = KeePassNatMsgExt.HostInstance;
            _ext = KeePassNatMsgExt.ExtInstance;
        }

        public bool UpdateEntry(string uuid, string username, string password, string formHost)
        {
            PwEntry entry = null;
            PwUuid id = new PwUuid(MemUtil.HexStringToByteArray(uuid));
            PwDatabase db = null;
            var configOpt = new ConfigOpt(_host.CustomConfig);
            if (configOpt.SearchInAllOpenedDatabases)
            {
                foreach (PwDocument doc in _host.MainWindow.DocumentManager.Documents)
                {
                    if (doc.Database.IsOpen)
                    {
                        entry = doc.Database.RootGroup.FindEntry(id, true);
                        if (entry != null)
                        {
                            db = doc.Database;
                            break;
                        }
                    }
                }
            }
            else
            {
                entry = _host.Database.RootGroup.FindEntry(id, true);
                db = _host.Database;
            }

            if (entry == null)
            {
                return false;
            }

            string[] up = _ext.GetUserPass(entry);
            var u = up[0];
            var p = up[1];

            if (u != username || p != password)
            {
                bool allowUpdate = configOpt.AlwaysAllowUpdates;

                if (!allowUpdate)
                {
                    _host.MainWindow.Activate();

                    DialogResult result;
                    if (_host.MainWindow.IsTrayed())
                    {
                        result = MessageBox.Show(
                            String.Format("Do you want to update the information in {0} - {1}?", formHost, u),
                            "Update Entry", MessageBoxButtons.YesNo,
                            MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    else
                    {
                        result = MessageBox.Show(
                            _host.MainWindow,
                            String.Format("Do you want to update the information in {0} - {1}?", formHost, u),
                            "Update Entry", MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }


                    if (result == DialogResult.Yes)
                    {
                        allowUpdate = true;
                    }
                }

                if (allowUpdate)
                {
                    PwObjectList<PwEntry> m_vHistory = entry.History.CloneDeep();
                    entry.History = m_vHistory;
                    entry.CreateBackup(null);

                    entry.Strings.Set(PwDefs.UserNameField, new ProtectedString(false, username));
                    entry.Strings.Set(PwDefs.PasswordField, new ProtectedString(true, password));
                    entry.Touch(true, false);
                    _ext.UpdateUI(entry.ParentGroup);

                    AutoSaveIfRequired(db);

                    return true;
                }
            }

            return false;
        }

        private void AutoSaveIfRequired(PwDatabase db)
        {
            if (!KeePass.Program.Config.Application.AutoSaveAfterEntryEdit) return;
            _host.MainWindow.Invoke(new MethodInvoker(() =>
            { //different thread access UI elements
                KeePassNatMsgExt.HostInstance.MainWindow.SaveDatabase(db, null);
            }));
        }

        public bool CreateEntry(string username, string password, string url, string submithost, string realm, string groupUuid)
        {
            string baseUrl = url;
            // index bigger than https:// <-- this slash
            if (baseUrl.LastIndexOf("/") > 9)
            {
                baseUrl = baseUrl.Substring(0, baseUrl.LastIndexOf("/") + 1);
            }

            var uri = new Uri(url);

            PwEntry entry = new PwEntry(true, true);
            entry.Strings.Set(PwDefs.TitleField, new ProtectedString(false, uri.Host));
            entry.Strings.Set(PwDefs.UserNameField, new ProtectedString(false, username));
            entry.Strings.Set(PwDefs.PasswordField, new ProtectedString(true, password));
            entry.Strings.Set(PwDefs.UrlField, new ProtectedString(true, baseUrl));

            if ((submithost != null && uri.Host != submithost) || realm != null)
            {
                var config = new EntryConfig();
                if (submithost != null)
                    config.Allow.Add(submithost);
                if (realm != null)
                    config.Realm = realm;

                var serializer = _ext.NewJsonSerializer();
                var writer = new StringWriter();
                serializer.Serialize(writer, config);
                entry.Strings.Set(KeePassNatMsgExt.KeePassNatMsgName, new ProtectedString(false, writer.ToString()));
            }

            PwGroup group = null;

            if (!string.IsNullOrEmpty(groupUuid))
            {
                var db = _ext.GetConnectionDatabase();
                if (db.RootGroup != null)
                {
                    var uuid = new PwUuid(MemUtil.HexStringToByteArray(groupUuid));
                    group = db.RootGroup.FindGroup(uuid, true);
                }
            }

            if (group == null)
                group = _ext.GetPasswordsGroup();

            group.AddEntry(entry, true);
            _ext.UpdateUI(group);

            AutoSaveIfRequired(_ext.GetConnectionDatabase());

            return true;
        }
    }
}
