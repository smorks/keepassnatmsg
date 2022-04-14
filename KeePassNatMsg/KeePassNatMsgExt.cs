using KeePass;
using KeePass.Plugins;
using KeePass.UI;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Security;
using KeePassLib.Utility;
using KeePassNatMsg.Entry;
using KeePassNatMsg.Protocol;
using KeePassNatMsg.Protocol.Action;
using KeePassNatMsg.Protocol.Crypto;
using KeePassNatMsg.Protocol.Listener;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeePass.Forms;
using KeePassNatMsg.Ui;
using OptionsForm = KeePassNatMsg.Options.OptionsForm;

namespace KeePassNatMsg
{
    public sealed class KeePassNatMsgExt : Plugin, IDisposable
    {

        /// <summary>
        /// an arbitrarily generated uuid for the keepassnatmsg root entry
        /// </summary>
        public static readonly byte[] KeePassNatMsgUuid = {
                0x34, 0x69, 0x7a, 0x40, 0x8a, 0x5b, 0x41, 0xc0,
                0x9f, 0x36, 0x89, 0x7d, 0x62, 0x3e, 0xcb, 0x31
        };

        /// <summary>
        /// an arbitrarily generated uuid for the keepassnatmsg new password group
        /// </summary>
        public static readonly byte[] KeePassNatMsgGroupUuid = {
                0xa4, 0x36, 0xb6, 0x24, 0xee, 0x2c, 0x44, 0x21,
                0xb3, 0xe0, 0x94, 0x99, 0x24, 0xe9, 0xc1, 0x8c
        };

        internal static IPluginHost HostInstance;
        internal static KeePassNatMsgExt ExtInstance;
        internal static Helper CryptoHelper;

        private const int DefaultNotificationTime = 5000;
        private const string KeePassNatMsgSettings = "KeePassNatMsg Settings";
        private const string KeePassXcSettings = "KeePassXC-Browser Settings";
        private const string KeePassNatMsgDbKey = "KeePassNatMsgDbKey_";
        private const string KeePassXcDbKey = "KPXC_BROWSER_";
        public const string KeePassNatMsgNameLegacy = "KeePassHttp Settings";
        public const string KeePassNatMsgGroupName = "KeePassNatMsg Passwords";
        public const string KeePassNatMsgLegacyMigrated = "KeePassNatMsg_Migrated";
        public const string AssociateKeyPrefix = "Public Key: ";
        private const string PipeName = "kpxc_server";

        private static readonly Version KeePassXcVersion = new Version(2, 6, 6);

        private IListener _listener;

        public override string UpdateUrl
        {
            get
            {
                return "https://dev.brandt.tech/keepass-plugin.txt";
            }
        }

        private Handlers _handlers;
        private bool _isLocked;

        internal static string SettingKey
        {
            get
            {
                var opts = new ConfigOpt(HostInstance.CustomConfig);
                return opts.UseKeePassXcSettings ? KeePassXcSettings : KeePassNatMsgSettings;
            }
        }

        internal static string DbKey
        {
            get
            {
                var opts = new ConfigOpt(HostInstance.CustomConfig);
                return GetDbKey(opts.UseKeePassXcSettings);
            }
        }

        internal static string GetDbKey(bool useKpxc)
        {
            return useKpxc ? KeePassXcDbKey : KeePassNatMsgDbKey;
        }

        private PwEntry GetConfigEntryLegacy(PwDatabase db)
        {
            var root = db.RootGroup;
            var uuid = new PwUuid(KeePassNatMsgUuid);
            return root.FindEntry(uuid, false);
        }

        internal PwGroup GetPasswordsGroup()
        {
            var root = GetConnectionDatabase().RootGroup;
            var uuid = new PwUuid(KeePassNatMsgGroupUuid);
            var group = root.FindGroup(uuid, true);
            if (group == null)
            {
                group = new PwGroup(false, true, KeePassNatMsgGroupName, PwIcon.WorldComputer);
                group.Uuid = uuid;
                root.AddGroup(group, true);
                UpdateUI(null);
            }
            return group;
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
                var notify = HostInstance.MainWindow.MainNotifyIcon;
                if (notify == null)
                    return;

                EventHandler clicked = null;
                EventHandler closed = null;

                clicked = delegate
                {
                    notify.BalloonTipClicked -= clicked;
                    notify.BalloonTipClosed -= closed;
                    if (onclick != null) onclick.Invoke(notify, null);
                };
                closed = delegate
                {
                    notify.BalloonTipClicked -= clicked;
                    notify.BalloonTipClosed -= closed;
                    if (onclose != null) onclose.Invoke(notify, null);
                };

                //notify.BalloonTipIcon = ToolTipIcon.Info;
                notify.BalloonTipTitle = "KeePassNatMsg";
                notify.BalloonTipText = text;
                notify.ShowBalloonTip(DefaultNotificationTime);
                // need to add listeners after showing, or closed is sent right away
                notify.BalloonTipClosed += closed;
                notify.BalloonTipClicked += clicked;
            };
            if (HostInstance.MainWindow.InvokeRequired)
                HostInstance.MainWindow.Invoke(m);
            else
                m.Invoke();
        }

        public override bool Initialize(IPluginHost pluginHost)
        {
            HostInstance = pluginHost;
            ExtInstance = this;
            CryptoHelper = new Helper();

            GlobalWindowManager.WindowAdded += GlobalWindowManagerOnWindowAdded;
            GlobalWindowManager.WindowRemoved += GlobalWindowManagerOnWindowRemoved;

            var optionsMenu = new ToolStripMenuItem("KeePassNatMsg Options...");
            optionsMenu.Click += OnOptions_Click;
            optionsMenu.Image = Properties.Resources.earth_lock;
            //optionsMenu.Image = global::KeePass.Properties.Resources.B16x16_File_Close;
            HostInstance.MainWindow.ToolsMenu.DropDownItems.Add(optionsMenu);

            pluginHost.MainWindow.FileClosingPre += MainWindow_FileClosingPre;
            pluginHost.MainWindow.FileOpened += MainWindow_FileOpened;

            try
            {
                _handlers = new Handlers();
                _handlers.Initialize();

                // check if we're running under Mono
                var t = Type.GetType("Mono.Runtime");

                if (t == null)
                {
                    // not Mono, assume Windows
                    _listener = new NamedPipeListener(string.Format("keepassxc\\{0}\\{1}", Environment.UserName, PipeName));
                    _listener.MessageReceived += Listener_MessageReceived;
                    _listener.Start();
                }
                else
                {
                    if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
                    {
                        _listener = new UnixSocketListener();
                        _listener.MessageReceived += Listener_MessageReceived;
                        _listener.Start();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(HostInstance.MainWindow, e.ToString(), "Unable to start", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return true;
        }

        private void GlobalWindowManagerOnWindowRemoved(object sender, GwmWindowEventArgs e)
        {
            var form = e.Form as PwEntryForm;
            if (form == null) return;

            form.Shown -= PwEntryForm_Shown;
            form.Resize -= PwEntryForm_Resize;
        }

        private void GlobalWindowManagerOnWindowAdded(object sender, GwmWindowEventArgs e)
        {
            var form = e.Form as PwEntryForm;
            if (form == null) return;

            form.Shown += PwEntryForm_Shown;
            form.Resize += PwEntryForm_Resize;
        }

        private void PwEntryForm_Resize(object sender, EventArgs e)
        {
        }

        private void PwEntryForm_Shown(object sender, EventArgs e)
        {
            var form = sender as PwEntryForm;
            if (form == null) return;

            var _ = new PwEntryFormExt(form);
        }

        private void MainWindow_FileOpened(object sender, KeePass.Forms.FileOpenedEventArgs e)
        {
            if (_isLocked)
            {
                _isLocked = false;

                var resp = new Response(Actions.DATABASE_UNLOCKED);

                _listener.Write(resp.GetEncryptedResponse());
            }

            PromptToMigrate(e.Database);
        }

        private void MainWindow_FileClosingPre(object sender, KeePass.Forms.FileClosingEventArgs e)
        {
            if (e.Flags == KeePass.Forms.FileEventFlags.Locking)
            {
                _isLocked = true;

                var resp = new Response(Actions.DATABASE_LOCKED);

                _listener.Write(resp.GetEncryptedResponse());
            }
        }

        private void Listener_MessageReceived(object sender, PipeMessageReceivedEventArgs e)
        {
            var req = Request.FromString(e.Message);
            var resp = _handlers.ProcessRequest(req);
            if (resp != null)
            {
                e.Writer.Send(resp.GetEncryptedResponse());
            }
        }

        void OnOptions_Click(object sender, EventArgs e)
        {
            var form = new OptionsForm(new ConfigOpt(HostInstance.CustomConfig));
            UIUtil.ShowDialogAndDestroy(form);
        }

        internal JsonSerializer NewJsonSerializer()
        {
            var settings = new JsonSerializerSettings();
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            settings.NullValueHandling = NullValueHandling.Ignore;

            return JsonSerializer.Create(settings);
        }

        public override void Terminate()
        {
            if (_listener != null)
                _listener.Stop();
        }

        internal void UpdateUI(PwGroup group)
        {
            var win = HostInstance.MainWindow;
            if (group == null) group = GetConnectionDatabase().RootGroup;
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
            return GetUserPass(new PwEntryDatabase(entry, HostInstance.Database));
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
                HostInstance.MainWindow.UpdateUI(false, null, false, null, false, null, false);
            };
            if (HostInstance.MainWindow.InvokeRequired)
                HostInstance.MainWindow.Invoke(f);
            else
                f.Invoke();

            return new string[] { user, pass };
        }

        internal string GetDbHash(PwDatabase db)
        {
            return GetDbHash(db, true);
        }

        private string GetDbHash(PwDatabase db, bool useNatMsg)
        {
            if (useNatMsg)
            {
                var ms = new MemoryStream();
                ms.Write(db.RootGroup.Uuid.UuidBytes, 0, 16);
                ms.Write(db.RecycleBinUuid.UuidBytes, 0, 16);
                return Hash(ms.ToArray());
            }

            // KeePassXC's method for generating the db hash
            var utf8 = new System.Text.UTF8Encoding(false);
            var data = utf8.GetBytes(db.RootGroup.Uuid.ToHexString().ToLower());
            return Hash(data).ToLower();
        }

        private string Hash(byte[] data)
        {
            var hashProvider = new SHA256CryptoServiceProvider();
            return ByteToHexBitFiddle(hashProvider.ComputeHash(data));
        }

        internal string GetDbHashForMessage()
        {
            var opts = new ConfigOpt(HostInstance.CustomConfig);
            return GetDbHash(GetConnectionDatabase(), !opts.UseKeePassXcSettings);
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
                var win = HostInstance.MainWindow;
                win.Invoke((MethodInvoker)delegate
                {
                    f.Activate();
                    f.Icon = win.Icon;
                    f.Key = key;
                    f.Load += delegate { f.Activate(); };
                    f.ShowDialog(win);

                    if (f.KeyId != null)
                    {
                        bool keyNameExists = true;
                        var db = GetConnectionDatabase();
                        var customKey = DbKey + f.KeyId;

                        while (keyNameExists)
                        {
                            DialogResult keyExistsResult = DialogResult.Yes;
                            if (db.CustomData.Exists(customKey))
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
                            db.CustomData.Set(customKey, key);
                            id = f.KeyId;
                            UpdateUI(null);
                        }
                    }
                });
            }
            return id;
        }

        internal EntryConfig GetEntryConfig(PwEntry e)
        {
            var serializer = NewJsonSerializer();
            if (e.CustomData.Exists(SettingKey))
            {
                var json = e.CustomData.Get(SettingKey);
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
            e.CustomData.Set(SettingKey, writer.ToString());
            e.Touch(true);
            UpdateUI(e.ParentGroup);
        }

        internal JObject GeneratePassword()
        {
            byte[] pbEntropy = null;
            ProtectedString psNew;
            PwProfile autoProfile = Program.Config.PasswordGenerator.AutoGeneratedPasswordsProfile;
            PwGenerator.Generate(out psNew, autoProfile, pbEntropy, Program.PwGeneratorPool);

            byte[] pbNew = psNew.ReadUtf8();
            if (pbNew != null)
            {
                uint uBits = QualityEstimation.EstimatePasswordBits(pbNew);
                var item = new JObject { { "bits", uBits }, { "password", StrUtil.Utf8.GetString(pbNew) } };
                MemUtil.ZeroByteArray(pbNew);
                return item;
            }
            return null;
        }

        public static string GetVersion()
        {
            var c = new ConfigOpt(HostInstance.CustomConfig);
            var verOvr = c.OverrideKeePassXcVersion;
            return string.IsNullOrWhiteSpace(verOvr) ? KeePassXcVersion.ToString() : verOvr;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                IDisposable disposable = (IDisposable)_listener;
                if (disposable != null)
                    disposable.Dispose();
            }
            // free native resources
        }

        public PwDatabase GetConnectionDatabase()
        {
            var options = new ConfigOpt(HostInstance.CustomConfig);
            if (string.IsNullOrEmpty(options.ConnectionDatabaseHash))
            {
                return HostInstance.Database;
            }
            else
            {
                var document = HostInstance.MainWindow.DocumentManager.Documents.Find(p => GetDbHash(p.Database) == options.ConnectionDatabaseHash);
                if (document != null)
                    return document.Database;
                else
                    return HostInstance.Database;
            }
        }

        internal void PromptToMigrate(PwDatabase db)
        {
            if (db.IsOpen && HasLegacyConfig(db))
            {
                var result = MessageBox.Show(
                    HostInstance.MainWindow,
                    "Your current KeePassNatMsg connection keys and entry settings need to be migrated to Custom Data. It is strongly recommended that you backup your database before proceeding. Do you wish to proceed with the migration?",
                    "Migrate KeePassNatMsg Settings?",
                    MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    MigrateLegacyConfig(db);
                    MessageBox.Show(
                        HostInstance.MainWindow,
                        string.Format("Your settings have been migrated. Please manually remove the \"{0}\" entry once you have verified everything is working as intended.", KeePassNatMsgNameLegacy),
                        "Migration Successful");
                }
            }
        }

        internal bool HasLegacyConfig(PwDatabase db)
        {
            var config = GetConfigEntryLegacy(db);

            return config != null && !db.CustomData.Exists(KeePassNatMsgLegacyMigrated);
        }

        internal void MigrateLegacyConfig(PwDatabase db)
        {
            // get database keys
            var config = GetConfigEntryLegacy(db);

            if (config != null)
            {
                var keys = new List<string>();

                foreach (var s in config.Strings)
                {
                    if (s.Key.StartsWith(AssociateKeyPrefix))
                    {
                        keys.Add(s.Key);
                    }
                }

                // move database keys
                foreach (var key in keys)
                {
                    var id = key.Substring(AssociateKeyPrefix.Length).Trim();
                    var customKey = DbKey + id;
                    db.CustomData.Set(customKey, config.Strings.ReadSafe(key));
                    config.Strings.Remove(key);
                }
            }

            var listEntries = new PwObjectList<PwEntry>();
            db.RootGroup.SearchEntries(new SearchParameters
            {
                SearchInStringNames = true,
                SearchString = KeePassNatMsgNameLegacy,
            }, listEntries);

            // move entry allow/deny config
            if (listEntries.UCount > 0)
            {
                foreach (var entry in listEntries)
                {
                    if (entry.Strings.Exists(KeePassNatMsgNameLegacy))
                    {
                        var json = entry.Strings.ReadSafe(KeePassNatMsgNameLegacy);
                        entry.CustomData.Set(SettingKey, json);
                        entry.Strings.Remove(KeePassNatMsgNameLegacy);
                    }
                }
            }

            db.CustomData.Set(KeePassNatMsgLegacyMigrated, DateTime.UtcNow.ToString("u"));

            // set db modified
            HostInstance.MainWindow.UpdateUI(false, null, false, null, false, null, true);
        }

        internal bool HasConfig(PwDatabase db, bool useKpnm)
        {
            var dbKey = useKpnm ? KeePassNatMsgDbKey : KeePassXcDbKey;
            var settings = useKpnm ? KeePassNatMsgSettings : KeePassXcSettings;

            var hasCustomData = db.CustomData.Where(x => x.Key.StartsWith(dbKey)).Any();
            var hasEntries = db.RootGroup.GetEntries(true).Where(x => x.CustomData.Exists(settings)).Any();

            return hasCustomData || hasEntries;
        }

        internal void MoveConfig(PwDatabase db, bool fromKpnm)
        {
            var fromDbKey = fromKpnm ? KeePassNatMsgDbKey : KeePassXcDbKey;
            var fromSettings = fromKpnm ? KeePassNatMsgSettings : KeePassXcSettings;
            var toDbKey = fromKpnm ? KeePassXcDbKey : KeePassNatMsgDbKey;
            var toSettings = fromKpnm ? KeePassXcSettings : KeePassNatMsgSettings;
            var dupeKeys = new List<string>();
            var dupeEntries = new List<PwEntry>();

            // first, move database keys
            var oldCustomData = db.CustomData.Where(x => x.Key.StartsWith(fromDbKey)).ToList();

            foreach (var cd in oldCustomData)
            {
                var id = cd.Key.Substring(fromDbKey.Length).Trim();
                var newKey = toDbKey + id;
                if (!db.CustomData.Exists(newKey))
                {
                    db.CustomData.Set(newKey, cd.Value);
                    db.CustomData.Remove(cd.Key);
                }
                else
                {
                    dupeKeys.Add(id);
                }
            }

            var entries = db.RootGroup.GetEntries(true).Where(x => x.CustomData.Exists(fromSettings));

            foreach (var e in entries)
            {
                var json = e.CustomData.Get(fromSettings);
                if (!e.CustomData.Exists(toSettings))
                {
                    e.CustomData.Set(toSettings, json);
                    e.CustomData.Remove(fromSettings);
                }
                else
                {
                    dupeEntries.Add(e);
                }
            }

            if (dupeKeys.Count > 0 || dupeEntries.Count > 0)
            {
                var lines = new List<string>();

                if (dupeKeys.Count > 0)
                {
                    lines.AddRange(new[] { string.Empty, "Duplicate Keys:", string.Empty });
                    lines.AddRange(dupeKeys);
                    lines.Add(string.Empty);
                }

                if (dupeEntries.Count > 0)
                {
                    lines.AddRange(new[] { string.Empty, "Duplicate Entry Configs:", string.Empty });
                    lines.AddRange(dupeEntries.Select(x => x.Strings.ReadSafe(PwDefs.TitleField)));
                    lines.Add(string.Empty);
                }

                var file = Path.Combine(Path.GetTempPath(), string.Format("KeePassNatMsg-Migration-{0:yyyy-MM-dd-HH-mm-ss}.log", DateTime.Now));

                File.WriteAllLines(file, lines, new System.Text.UTF8Encoding(false));

                var title = dupeKeys.Count > 0 && dupeEntries.Count > 0 ? "Duplicate Keys/Entries Found" : dupeKeys.Count > 0 ? "Duplicate Keys Found" : "Duplicate Entries Found";
                var result = MessageBox.Show("There were duplicates found. Please check the log file for more information:\r\n\r\n" + file + "\r\n\r\nDo you want to open the log file now?", title, MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    var psi = new System.Diagnostics.ProcessStartInfo(file);
                    psi.UseShellExecute = true;
                    psi.Verb = "open";
                    System.Diagnostics.Process.Start(psi);
                }
            }

            // set db modified
            HostInstance.MainWindow.UpdateUI(false, null, false, null, false, null, true);
        }
    }
}
