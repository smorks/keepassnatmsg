using KeePassLib;
using KeePassNatMsg.NativeMessaging;
using KeePassNatMsg.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassNatMsg.Options
{
    public partial class OptionsForm : Form
    {
        readonly ConfigOpt _config;
        private bool _restartRequired = false;
        private readonly NativeMessagingHost _host;

        private string AssemblyVersion
        {
            get
            {
                try
                {
                    return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                catch { }

                return "unknown";
            }
        }

        public OptionsForm(ConfigOpt config)
        {
            _host = NativeMessagingHost.GetHost();
            _config = config;
            InitializeComponent();
            lblVersion.Text = string.Format("KeePassNatMsg v{0}", AssemblyVersion);
        }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            credNotifyCheckbox.Checked = _config.ReceiveCredentialNotification;
            credMatchingCheckbox.Checked = _config.SpecificMatchingOnly;
            unlockDatabaseCheckbox.Checked = _config.UnlockDatabaseRequest;
            credAllowAccessCheckbox.Checked = _config.AlwaysAllowAccess;
            credAllowUpdatesCheckbox.Checked = _config.AlwaysAllowUpdates;
            if (_config.SearchInAllOpenedDatabases)
            {
                // Only for backward compatibility
                credSearchInAllOpenedDatabasesRadioButton.Checked = true;
                _config.SearchInAllOpenedDatabases = false;
            }
            else
            {
                credOnlySearchInSelectedDatabaseRadioButton.Checked = (_config.AllowSearchDatabase == (ulong)AllowSearchDatabase.SearchInOnlySelectedDatabase);
                credSearchInAllOpenedDatabasesRadioButton.Checked = (_config.AllowSearchDatabase == (ulong)AllowSearchDatabase.SearchInAllOpenedDatabases);
                credRestrictSearchInSpecificDatabaseRadioButton.Checked = (_config.AllowSearchDatabase == (ulong)AllowSearchDatabase.RestrictSearchInSpecificDatabase);
            }
            comboBoxSearchDatabases.Enabled = credRestrictSearchInSpecificDatabaseRadioButton.Checked;
            hideExpiredCheckbox.Checked = _config.HideExpired;
            matchSchemesCheckbox.Checked = _config.MatchSchemes;
            returnStringFieldsCheckbox.Checked = _config.ReturnStringFields;
            returnStringFieldsWithKphOnlyCheckBox.Checked = _config.ReturnStringFieldsWithKphOnly;
            SortByUsernameRadioButton.Checked = _config.SortResultByUsername;
            SortByTitleRadioButton.Checked = !_config.SortResultByUsername;
            txtKPXCVerOverride.Text = _config.OverrideKeePassXcVersion;
            chkSearchUrls.Checked = _config.SearchUrls;
            chkUseKpxcSettingsKey.Checked = _config.UseKeePassXcSettings;
            chkUseLegacyHostMatching.Checked = _config.UseLegacyHostMatching;

            this.returnStringFieldsCheckbox_CheckedChanged(null, EventArgs.Empty);

            InitDatabasesDropdown();
            foreach (DatabaseItem item in comboBoxSearchDatabases.Items)
            {
                if (item.DbHash == _config.SearchDatabaseHash)
                {
                    comboBoxSearchDatabases.SelectedItem = item;
                }
            }
            foreach (DatabaseItem item in comboBoxDatabases.Items)
            {
                if (item.DbHash == _config.ConnectionDatabaseHash)
                {
                    comboBoxDatabases.SelectedItem = item;
                }
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _config.ReceiveCredentialNotification = credNotifyCheckbox.Checked;
            _config.SpecificMatchingOnly = credMatchingCheckbox.Checked;
            _config.UnlockDatabaseRequest = unlockDatabaseCheckbox.Checked;
            _config.AlwaysAllowAccess = credAllowAccessCheckbox.Checked;
            _config.AlwaysAllowUpdates = credAllowUpdatesCheckbox.Checked;
            _config.SearchDatabaseHash = (comboBoxSearchDatabases.SelectedItem as DatabaseItem) == null ? null : (comboBoxSearchDatabases.SelectedItem as DatabaseItem).DbHash;
            _config.HideExpired = hideExpiredCheckbox.Checked;
            _config.MatchSchemes = matchSchemesCheckbox.Checked;
            _config.ReturnStringFields = returnStringFieldsCheckbox.Checked;
            _config.ReturnStringFieldsWithKphOnly = returnStringFieldsWithKphOnlyCheckBox.Checked;
            _config.SortResultByUsername = SortByUsernameRadioButton.Checked;
            _config.OverrideKeePassXcVersion = txtKPXCVerOverride.Text;
            _config.ConnectionDatabaseHash = (comboBoxDatabases.SelectedItem as DatabaseItem) == null ? null : (comboBoxDatabases.SelectedItem as DatabaseItem).DbHash;
            _config.SearchUrls = chkSearchUrls.Checked;
            _config.UseLegacyHostMatching = chkUseLegacyHostMatching.Checked;

            if (_config.UseKeePassXcSettings != chkUseKpxcSettingsKey.Checked)
            {
                MigrateSettings(true);
            }

            _config.UseKeePassXcSettings = chkUseKpxcSettingsKey.Checked;

            if (_restartRequired)
            {
                MessageBox.Show(
                    "You have successfully changed the port number and/or the host name.\nA restart of KeePass is required!\n\nPlease restart KeePass now.",
                    "Restart required!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void removePermissionsButton_Click(object sender, EventArgs e)
        {
            if (KeePass.Program.MainForm.DocumentManager.ActiveDatabase.IsOpen)
            {
                PwDatabase db = KeePass.Program.MainForm.DocumentManager.ActiveDatabase;

                uint counter = 0;
                var entries = db.RootGroup.GetEntries(true);

                if (entries.Count() > 999)
                {
                    MessageBox.Show(
                        String.Format("{0} entries detected!\nSearching and removing permissions could take some while.\n\nWe will inform you when the process has been finished.", entries.Count().ToString()),
                        String.Format("{0} entries detected", entries.Count().ToString()),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                foreach (var entry in entries)
                {
                    foreach (var str in entry.CustomData)
                    {
                        if (str.Key.Equals(KeePassNatMsgExt.SettingKey))
                        {
                            entry.History = entry.History.CloneDeep();
                            entry.CreateBackup(null);
                            entry.CustomData.Remove(str.Key);
                            entry.Touch(true);

                            counter++;

                            break;
                        }
                    }
                }

                if (counter > 0)
                {
                    KeePass.Program.MainForm.UpdateUI(false, null, true, db.RootGroup, true, null, true);
                    MessageBox.Show(
                        String.Format("Successfully removed permissions from {0} entr{1}.", counter.ToString(), counter == 1 ? "y" : "ies"),
                        String.Format("Removed permissions from {0} entr{1}", counter.ToString(), counter == 1 ? "y" : "ies"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "The active database does not contain an entry with permissions.",
                        "No entry with permissions found!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            else
            {
                MessageBox.Show("The active database is locked!\nPlease unlock the selected database or choose another one which is unlocked.", "Database locked!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void returnStringFieldsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            this.returnStringFieldsWithKphOnlyCheckBox.Enabled = this.returnStringFieldsCheckbox.Checked;
        }

        private void btnInstallNativeMessaging_Click(object sender, EventArgs e)
        {
            var bsf = new BrowserSelectForm(_host);

            if (bsf.ShowDialog(this) == DialogResult.OK)
            {
                var t = new Task(() =>
                {
                    _host.Install(bsf.SelectedBrowsers);
                    _host.UpdateProxy();
                    GetNativeMessagingStatus();
                    Invoke(new Action(() => MessageBox.Show(this, "The native messaging host installed completed successfully.", "Install Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                });
                t.Start();
            }
        }

        private void CheckNativeMessagingHost()
        {
            var t = new Task<bool>(() => _host.GetBrowserStatuses().Any(bs => bs.Value == BrowserStatus.Installed));

            var t2 = t.ContinueWith((ti) =>
            {
                if (ti.IsCompleted && !ti.Result)
                {
                    Invoke(new Action(() => PromptInstall()));
                }
                GetNativeMessagingStatus();
            });

            SetProxyVersionText("Loading Native Messaging Status...");

            t.Start();
        }

        private void PromptInstall()
        {
            var nmiInstall = MessageBox.Show(this, "The native messaging host was not detected. It must be installed for KeePassNatMsg to work. Do you want to install it now?", "Native Messaging Host Not Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (nmiInstall == DialogResult.Yes)
            {
                var bsf = new BrowserSelectForm(_host);
                if (bsf.ShowDialog(this) == DialogResult.OK)
                {
                    _host.Install(bsf.SelectedBrowsers);
                    _host.UpdateProxy();
                    MessageBox.Show(this, "The native messaging host installed completed successfully.", "Install Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void OptionsForm_Shown(object sender, EventArgs e)
        {
            CheckNativeMessagingHost();
        }

        private void GetNativeMessagingStatus()
        {
            var statuses = _host.GetBrowserStatuses();
            var lst = new List<string>();

            foreach (var b in statuses.Keys)
            {
                lst.Add(string.Format("{0}: {1}", b.GetDescription(), statuses[b].GetDescription()));
            }

            var latestVersion = _host.GetLatestProxyVersion();
            var proxyVersion = _host.GetProxyVersion();
            var proxyDisplay = proxyVersion == null ? "Not Installed" : proxyVersion.ToString();
            var latestVersionDisplay = string.Empty;

            if (proxyVersion != null && latestVersion != null)
            {
                if (latestVersion > proxyVersion)
                {
                    latestVersionDisplay = " New Version Available: " + latestVersion;
                }
                else
                {
                    latestVersionDisplay = " (Up To Date)";
                }
            }

            lst.Add(string.Format("Proxy: {0}{1}", proxyDisplay, latestVersionDisplay));

            SetProxyVersionText(string.Join(Environment.NewLine, lst));
        }

        private void SetProxyVersionText(string text)
        {
            if (lblProxyVersion.InvokeRequired)
            {
                lblProxyVersion.Invoke(new Action<string>((x) => SetProxyVersionText(x)), text);
            }
            else
            {
                lblProxyVersion.Text = text;
            }
        }

        private void InitDatabasesDropdown()
        {
            foreach (var item in KeePass.Program.MainForm.DocumentManager.Documents)
            {
                if (!item.Database.IsOpen)
                    continue;

                var dbIdentifier = item.Database.Name;
                if (string.IsNullOrEmpty(dbIdentifier))
                {
                    dbIdentifier = item.Database.IOConnectionInfo.Path;
                }

                comboBoxSearchDatabases.Items.Add(new DatabaseItem { Id = dbIdentifier, DbHash = KeePassNatMsgExt.ExtInstance.GetDbHash(item.Database) });
                comboBoxDatabases.Items.Add(new DatabaseItem { Id = dbIdentifier, DbHash = KeePassNatMsgExt.ExtInstance.GetDbHash(item.Database) });
            }
        }

        private void LoadDatabaseKeys()
        {
            LoadDatabaseKeys(KeePass.Program.MainForm.DocumentManager.ActiveDatabase);
        }

        private void LoadDatabaseKeys(PwDatabase db)
        {
            if (db.IsOpen)
            {
                var keys = new List<DatabaseKeyItem>();
                var dbKey = KeePassNatMsgExt.GetDbKey(_config.UseKeePassXcSettings);

                foreach (var cd in db.CustomData)
                {
                    if (cd.Key.StartsWith(dbKey))
                    {
                        var keyName = cd.Key.Substring(dbKey.Length);
                        keys.Add(new DatabaseKeyItem { Name = keyName, Key = cd.Value });
                    }
                }

                dgvKeys.DataSource = keys;
            }
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == tabPage3)
            {
                LoadDatabaseKeys();
            }
        }

        private void btnRemoveSelectedKeys_Click(object sender, EventArgs e)
        {
            var db = KeePass.Program.MainForm.DocumentManager.ActiveDatabase;

            if (db.IsOpen)
            {
                var dbKey = KeePassNatMsgExt.GetDbKey(_config.UseKeePassXcSettings);

                var items = dgvKeys.SelectedRows
                    .OfType<DataGridViewRow>()
                    .Select(x => dbKey + ((x.DataBoundItem as DatabaseKeyItem) == null ? string.Empty : (x.DataBoundItem as DatabaseKeyItem).Name));

                var deleteKeys = db.CustomData
                    .Where(x => items.Contains(x.Key))
                    .Select(x => x.Key).ToList();

                RemoveKeys(deleteKeys, db);
            }
        }

        private void btnRemoveAllKeys_Click(object sender, EventArgs e)
        {
            var db = KeePass.Program.MainForm.DocumentManager.ActiveDatabase;

            if (db.IsOpen)
            {
                var dbKey = KeePassNatMsgExt.GetDbKey(_config.UseKeePassXcSettings);

                var deleteKeys = db.CustomData
                    .Where(x => x.Key.StartsWith(dbKey))
                    .Select(x => x.Key).ToList();

                RemoveKeys(deleteKeys, db);
            }
            else
            {
                MessageBox.Show("The active database is locked!\nPlease unlock the selected database or choose another one which is unlocked.", "Database locked!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveKeys(List<string> keys, PwDatabase db)
        {
            if (keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    db.CustomData.Remove(key);
                }

                LoadDatabaseKeys(db);

                KeePass.Program.MainForm.UpdateUI(false, null, true, db.RootGroup, true, null, true);
                MessageBox.Show(
                    string.Format("Successfully removed {0} encryption-key{1} from KeePassNatMsg Settings.", keys.Count, keys.Count == 1 ? "" : "s"),
                    string.Format("Removed {0} key{1} from database", keys.Count, keys.Count == 1 ? "" : "s"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                MessageBox.Show(
                    "No shared encryption-keys found in KeePassNatMsg Settings.", "No keys found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private void btnCheckForLegacyConfig_Click(object sender, EventArgs e)
        {
            var ext = KeePassNatMsgExt.ExtInstance;
            var db = KeePass.Program.MainForm.DocumentManager.ActiveDatabase;

            if (!db.IsOpen)
            {
                MessageBox.Show(this, "The active database is not open, config cannot be migrated.", "Active Database Not Open");
                return;
            }

            if (ext.HasLegacyConfig(db))
            {
                ext.PromptToMigrate(db);
            }
            else
            {
                MessageBox.Show(this, "Legacy Configuration was not found, or the config has already been migrated for the active database.", "Legacy Config Not Found");
            }
        }

        private void btnMigrateSettings_Click(object sender, EventArgs e)
        {
            MigrateSettings(false);
        }

        private bool MigrateSettings(bool quiet)
        {
            var ext = KeePassNatMsgExt.ExtInstance;
            var db = KeePass.Program.MainForm.DocumentManager.ActiveDatabase;

            if (!db.IsOpen)
            {
                if (!quiet)
                    MessageBox.Show(this, "The active database is not open, config cannot be migrated.", "Active Database Not Open");
                return false;
            }

            var fromKpnm = chkUseKpxcSettingsKey.Checked;
            var from = fromKpnm ? "KeePassNatMsg" : "KeePassXC";
            var to = fromKpnm ? "KeePassXC" : "KeePassNatMsg";

            if (ext.HasConfig(db, fromKpnm))
            {
                var result = DialogResult.Yes;

                if (!quiet)
                {
                    result = MessageBox.Show(
                        this,
                        string.Format("CAUTION: This will move all {0} Settings to {1}. Any existing {1} settings will be overwritten. You should create a backup of the database before proceeding. Are you sure you want to migrate settings from {0} to {1}?", from, to),
                        "Confirm Migrate Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                }

                if (result == DialogResult.Yes)
                {
                    UseWaitCursor = true;
                    ext.MoveConfig(db, fromKpnm);
                    UseWaitCursor = false;
                }
            }
            else
            {
                if (!quiet)
                    MessageBox.Show(this, string.Format("No {0} Settings found.", from), "No Settings to be Migrated");

                return false;
            }

            return true;
        }

        private void rbSearchDatabase_CheckedChanged(object sender, EventArgs e)
        {
            if (credOnlySearchInSelectedDatabaseRadioButton.Checked)
                _config.AllowSearchDatabase = (ulong)AllowSearchDatabase.SearchInOnlySelectedDatabase;
            else if (credSearchInAllOpenedDatabasesRadioButton.Checked)
                _config.AllowSearchDatabase = (ulong)AllowSearchDatabase.SearchInAllOpenedDatabases;
             else 
                _config.AllowSearchDatabase = (ulong)AllowSearchDatabase.RestrictSearchInSpecificDatabase;

            this.comboBoxSearchDatabases.Enabled = this.credRestrictSearchInSpecificDatabaseRadioButton.Checked;
        }
    }
}
