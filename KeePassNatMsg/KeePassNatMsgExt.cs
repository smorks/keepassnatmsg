using KeePass;
using KeePass.Plugins;
using KeePass.UI;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Security;
using KeePassLib.Utility;
using KeePassNatMsg.Entry;
using KeePassNatMsg.Options;
using KeePassNatMsg.Protocol;
using KeePassNatMsg.Protocol.Action;
using KeePassNatMsg.Protocol.Crypto;
using KeePassNatMsg.Protocol.Listener;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

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
        public const string KeePassNatMsgName = "KeePassHttp Settings";
        public const string KeePassNatMsgGroupName = "KeePassNatMsg Passwords";
        public const string AssociateKeyPrefix = "Public Key: ";
        private const string PipeName = "kpxc_server";

        private static readonly Version KeePassXcVersion = new Version(2, 4, 3);

        private IListener _listener;

        public override string UpdateUrl => "https://dev.brandt.tech/keepass-plugin.txt";

        private Handlers _handlers;

        internal PwEntry GetConfigEntry(bool create)
        {
            var root = GetConnectionDatabase().RootGroup;
            var uuid = new PwUuid(KeePassNatMsgUuid);
            var entry = root.FindEntry(uuid, false);
            if (entry == null && create)
            {
                entry = new PwEntry(false, true);
                entry.Uuid = uuid;
                entry.Strings.Set(PwDefs.TitleField, new ProtectedString(false, KeePassNatMsgName));
                root.AddEntry(entry, true);
                UpdateUI(null);
            }
            return entry;
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

        private int GetNotificationTime()
        {
            var time = DefaultNotificationTime;
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
                var notify = HostInstance.MainWindow.MainNotifyIcon;
                if (notify == null)
                    return;

                EventHandler clicked = null;
                EventHandler closed = null;

                clicked = delegate
                {
                    notify.BalloonTipClicked -= clicked;
                    notify.BalloonTipClosed -= closed;
                    onclick?.Invoke(notify, null);
                };
                closed = delegate
                {
                    notify.BalloonTipClicked -= clicked;
                    notify.BalloonTipClosed -= closed;
                    onclose?.Invoke(notify, null);
                };

                //notify.BalloonTipIcon = ToolTipIcon.Info;
                notify.BalloonTipTitle = "KeePassNatMsg";
                notify.BalloonTipText = text;
                notify.ShowBalloonTip(GetNotificationTime());
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

            var optionsMenu = new ToolStripMenuItem("KeePassNatMsg Options...");
            optionsMenu.Click += OnOptions_Click;
            optionsMenu.Image = KeePassNatMsg.Properties.Resources.earth_lock;
            //optionsMenu.Image = global::KeePass.Properties.Resources.B16x16_File_Close;
            HostInstance.MainWindow.ToolsMenu.DropDownItems.Add(optionsMenu);

            try
            {
                _handlers = new Handlers();
                _handlers.Initialize();

                // check if we're running under Mono
                var t = Type.GetType("Mono.Runtime");

                if (t == null)
                {
                    // not Mono, assume Windows
                    _listener = new NamedPipeListener($"keepassxc\\{Environment.UserName}\\{PipeName}");
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
            _listener?.Stop();
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
            var ms = new MemoryStream();
            ms.Write(db.RootGroup.Uuid.UuidBytes, 0, 16);
            ms.Write(db.RecycleBinUuid.UuidBytes, 0, 16);
            var sha256 = new SHA256CryptoServiceProvider();
            var hashBytes = sha256.ComputeHash(ms.ToArray());
            return ByteToHexBitFiddle(hashBytes);
        }

        internal string GetDbHash()
        {
            return GetDbHash(GetConnectionDatabase());
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
                        var entry = GetConfigEntry(true);

                        bool keyNameExists = true;
                        while (keyNameExists)
                        {
                            DialogResult keyExistsResult = DialogResult.Yes;
                            if (entry.Strings.Any(x => x.Key == AssociateKeyPrefix + f.KeyId))
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
                            entry.Strings.Set(AssociateKeyPrefix + f.KeyId, new ProtectedString(true, key));
                            entry.Touch(true);
                            UpdateUI(null);
                            id = f.KeyId;
                        }
                    }
                });
            }
            return id;
        }

        internal EntryConfig GetEntryConfig(PwEntry e)
        {
            var serializer = NewJsonSerializer();
            if (e.Strings.Exists(KeePassNatMsgName))
            {
                var json = e.Strings.ReadSafe(KeePassNatMsgName);
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
            e.Strings.Set(KeePassNatMsgName, new ProtectedString(false, writer.ToString()));
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
                disposable?.Dispose();
            }
            // free native resources
        }

        private PwDatabase GetConnectionDatabase()
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
    }
}
