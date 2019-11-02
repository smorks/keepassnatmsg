using KeePassNatMsg.NativeMessaging;
using KeePassNatMsg.Utils;
using KeePassLib;
using KeePassLib.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassNatMsg
{
    public partial class OptionsForm : Form
    {
        readonly ConfigOpt _config;
        private bool _restartRequired = false;
        private NativeMessagingHost _host;

        public OptionsForm(ConfigOpt config)
        {
            _config = config;
            InitializeComponent();
        }


        private PwEntry GetConfigEntry(PwDatabase db)
        {
            var root = db.RootGroup;
            var uuid = new PwUuid(KeePassNatMsgExt.KeePassNatMsgUuid);
            var entry = root.FindEntry(uuid, false);
            return entry;
        }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            credNotifyCheckbox.Checked = _config.ReceiveCredentialNotification;
            credMatchingCheckbox.Checked = _config.SpecificMatchingOnly;
            unlockDatabaseCheckbox.Checked = _config.UnlockDatabaseRequest;
            credAllowAccessCheckbox.Checked = _config.AlwaysAllowAccess;
            credAllowUpdatesCheckbox.Checked = _config.AlwaysAllowUpdates;
            credSearchInAllOpenedDatabases.Checked = _config.SearchInAllOpenedDatabases;
            hideExpiredCheckbox.Checked = _config.HideExpired;
            matchSchemesCheckbox.Checked = _config.MatchSchemes;
            returnStringFieldsCheckbox.Checked = _config.ReturnStringFields;
            returnStringFieldsWithKphOnlyCheckBox.Checked = _config.ReturnStringFieldsWithKphOnly;
            SortByUsernameRadioButton.Checked = _config.SortResultByUsername;
            SortByTitleRadioButton.Checked = !_config.SortResultByUsername;
            txtKPXCVerOverride.Text = _config.OverrideKeePassXcVersion;

            this.returnStringFieldsCheckbox_CheckedChanged(null, EventArgs.Empty);

			InitDatabasesDropdown();
			foreach (dynamic item in comboBoxDatabases.Items)
			{
				if (item.DatabaseHash == _config.ConnectionDatabaseHash)
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
            _config.SearchInAllOpenedDatabases = credSearchInAllOpenedDatabases.Checked;
            _config.HideExpired = hideExpiredCheckbox.Checked;
            _config.MatchSchemes = matchSchemesCheckbox.Checked;
            _config.ReturnStringFields = returnStringFieldsCheckbox.Checked;
            _config.ReturnStringFieldsWithKphOnly = returnStringFieldsWithKphOnlyCheckBox.Checked;
            _config.SortResultByUsername = SortByUsernameRadioButton.Checked;
            _config.OverrideKeePassXcVersion = txtKPXCVerOverride.Text;
			_config.ConnectionDatabaseHash = (comboBoxDatabases.SelectedItem as dynamic).DatabaseHash;
			if (_restartRequired)
            {
                MessageBox.Show(
                    "You have successfully changed the port number and/or the host name.\nA restart of KeePass is required!\n\nPlease restart KeePass now.",
                    "Restart required!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (KeePass.Program.MainForm.DocumentManager.ActiveDatabase.IsOpen)
            {
                PwDatabase db = KeePass.Program.MainForm.DocumentManager.ActiveDatabase;
                var entry = GetConfigEntry(db);
                if (entry != null)
                {
                    List<string> deleteKeys = new List<string>();

                    foreach (var s in entry.Strings)
                    {
                        if (s.Key.IndexOf(KeePassNatMsgExt.AssociateKeyPrefix) == 0)
                        {
                            deleteKeys.Add(s.Key);
                        }
                    }


                    if (deleteKeys.Count > 0)
                    {
                        PwObjectList<PwEntry> m_vHistory = entry.History.CloneDeep();
                        entry.History = m_vHistory;
                        entry.CreateBackup(null);

                        foreach (var key in deleteKeys)
                        {
                            entry.Strings.Remove(key);
                        }

                        entry.Touch(true);
                        KeePass.Program.MainForm.UpdateUI(false, null, true, db.RootGroup, true, null, true);
                        MessageBox.Show(
                            String.Format("Successfully removed {0} encryption-key{1} from KeePassNatMsg Settings.", deleteKeys.Count.ToString(), deleteKeys.Count == 1 ? "" : "s"),
                            String.Format("Removed {0} key{1} from database", deleteKeys.Count.ToString(), deleteKeys.Count == 1 ? "" : "s"),
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
                else
                {
                    MessageBox.Show("The active database does not contain an entry of KeePassNatMsg Settings.", "KeePassNatMsg Settings not available!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("The active database is locked!\nPlease unlock the selected database or choose another one which is unlocked.", "Database locked!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                    foreach (var str in entry.Strings)
                    {
                        if (str.Key == KeePassNatMsgExt.KeePassNatMsgName)
                        {
                            PwObjectList<PwEntry> m_vHistory = entry.History.CloneDeep();
                            entry.History = m_vHistory;
                            entry.CreateBackup(null);

                            entry.Strings.Remove(str.Key);

                            entry.Touch(true);

                            counter += 1;

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
                    MessageBox.Show(this, "The native messaging host installed completed successfully.", "Install Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    var nmiInstall = MessageBox.Show(this, $"The native messaging host was not detected. It must be installed for KeePassNatMsg to work. Do you want to install it now?", "Native Messaging Host Not Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
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
                GetNativeMessagingStatus();
            });

            lblProxyVersion.Text = "Loading Native Messaging Status...";

            t.Start();
        }

        private void OptionsForm_Shown(object sender, EventArgs e)
        {
            _host = NativeMessagingHost.GetHost();

            CheckNativeMessagingHost();
        }

        private void GetNativeMessagingStatus()
        {
            var statuses = _host.GetBrowserStatuses();
            var lst = new List<string>();

            foreach (var b in statuses.Keys)
            {
                lst.Add($"{b.GetDescription()}: {statuses[b].GetDescription()}");
            }

            var latestVersion = _host.GetLatestProxyVersion();
            var proxyVersion = _host.GetProxyVersion();
            var proxyDisplay = proxyVersion == null ? "Not Installed" : proxyVersion.ToString();
            var latestVersionDisplay = string.Empty;

            if (proxyVersion != null && latestVersion != null)
            {
                if (latestVersion > proxyVersion)
                {
                    latestVersionDisplay = $" New Version Available: {latestVersion}";
                }
                else
                {
                    latestVersionDisplay = " (Up To Date)";
                }
            }

            lst.Add($"Proxy: {proxyDisplay}{latestVersionDisplay}");

            lblProxyVersion.Text = string.Join(Environment.NewLine, lst);
        }

		private void InitDatabasesDropdown()
		{
			foreach (var item in KeePass.Program.MainForm.DocumentManager.Documents)
			{
				var dbIdentifier = item.Database.Name;
				if (string.IsNullOrEmpty(dbIdentifier))
				{
					dbIdentifier = item.Database.IOConnectionInfo.Path;
				}

				comboBoxDatabases.Items.Add(new { DatabaseIdentifier = dbIdentifier, DatabaseHash = KeePassNatMsgExt.ExtInstance.GetDbHash(item.Database)});
			}
		}
	}
}
