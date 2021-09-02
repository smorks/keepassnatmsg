namespace KeePassNatMsg.Options
{
    partial class OptionsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblProxyVersion = new System.Windows.Forms.Label();
            this.btnInstallNativeMessaging = new System.Windows.Forms.Button();
            this.SortByUsernameRadioButton = new System.Windows.Forms.RadioButton();
            this.SortByTitleRadioButton = new System.Windows.Forms.RadioButton();
            this.hideExpiredCheckbox = new System.Windows.Forms.CheckBox();
            this.matchSchemesCheckbox = new System.Windows.Forms.CheckBox();
            this.removePermissionsButton = new System.Windows.Forms.Button();
            this.unlockDatabaseCheckbox = new System.Windows.Forms.CheckBox();
            this.credMatchingCheckbox = new System.Windows.Forms.CheckBox();
            this.credNotifyCheckbox = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnMigrateSettings = new System.Windows.Forms.Button();
            this.chkUseKpxcSettingsKey = new System.Windows.Forms.CheckBox();
            this.btnCheckForLegacyConfig = new System.Windows.Forms.Button();
            this.chkSearchUrls = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxDatabases = new System.Windows.Forms.ComboBox();
            this.txtKPXCVerOverride = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.returnStringFieldsWithKphOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.returnStringFieldsCheckbox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.credSearchInAllOpenedDatabases = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.credAllowUpdatesCheckbox = new System.Windows.Forms.CheckBox();
            this.credAllowAccessCheckbox = new System.Windows.Forms.CheckBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.scKeysMain = new System.Windows.Forms.SplitContainer();
            this.dgvKeys = new System.Windows.Forms.DataGridView();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnRemoveAllKeys = new System.Windows.Forms.Button();
            this.btnRemoveSelectedKeys = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scKeysMain)).BeginInit();
            this.scKeysMain.Panel1.SuspendLayout();
            this.scKeysMain.Panel2.SuspendLayout();
            this.scKeysMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKeys)).BeginInit();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(313, 497);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(88, 28);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.okButton.Location = new System.Drawing.Point(219, 497);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(88, 28);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "&Save";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(1, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(410, 487);
            this.tabControl1.TabIndex = 3;
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lblVersion);
            this.tabPage1.Controls.Add(this.lblProxyVersion);
            this.tabPage1.Controls.Add(this.btnInstallNativeMessaging);
            this.tabPage1.Controls.Add(this.SortByUsernameRadioButton);
            this.tabPage1.Controls.Add(this.SortByTitleRadioButton);
            this.tabPage1.Controls.Add(this.hideExpiredCheckbox);
            this.tabPage1.Controls.Add(this.matchSchemesCheckbox);
            this.tabPage1.Controls.Add(this.removePermissionsButton);
            this.tabPage1.Controls.Add(this.unlockDatabaseCheckbox);
            this.tabPage1.Controls.Add(this.credMatchingCheckbox);
            this.tabPage1.Controls.Add(this.credNotifyCheckbox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(402, 461);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVersion.Location = new System.Drawing.Point(174, 441);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(225, 17);
            this.lblVersion.TabIndex = 23;
            this.lblVersion.Text = "label7";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // lblProxyVersion
            // 
            this.lblProxyVersion.Location = new System.Drawing.Point(14, 236);
            this.lblProxyVersion.Name = "lblProxyVersion";
            this.lblProxyVersion.Size = new System.Drawing.Size(372, 129);
            this.lblProxyVersion.TabIndex = 22;
            this.lblProxyVersion.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // btnInstallNativeMessaging
            // 
            this.btnInstallNativeMessaging.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInstallNativeMessaging.Location = new System.Drawing.Point(14, 368);
            this.btnInstallNativeMessaging.Name = "btnInstallNativeMessaging";
            this.btnInstallNativeMessaging.Size = new System.Drawing.Size(372, 28);
            this.btnInstallNativeMessaging.TabIndex = 20;
            this.btnInstallNativeMessaging.Text = "Install/Update Native Messaging Host";
            this.btnInstallNativeMessaging.UseVisualStyleBackColor = true;
            this.btnInstallNativeMessaging.Click += new System.EventHandler(this.btnInstallNativeMessaging_Click);
            // 
            // SortByUsernameRadioButton
            // 
            this.SortByUsernameRadioButton.AutoSize = true;
            this.SortByUsernameRadioButton.Location = new System.Drawing.Point(7, 147);
            this.SortByUsernameRadioButton.Name = "SortByUsernameRadioButton";
            this.SortByUsernameRadioButton.Size = new System.Drawing.Size(171, 17);
            this.SortByUsernameRadioButton.TabIndex = 19;
            this.SortByUsernameRadioButton.TabStop = true;
            this.SortByUsernameRadioButton.Text = "Sort found entries by &username";
            this.SortByUsernameRadioButton.UseVisualStyleBackColor = true;
            // 
            // SortByTitleRadioButton
            // 
            this.SortByTitleRadioButton.AutoSize = true;
            this.SortByTitleRadioButton.Location = new System.Drawing.Point(7, 170);
            this.SortByTitleRadioButton.Name = "SortByTitleRadioButton";
            this.SortByTitleRadioButton.Size = new System.Drawing.Size(141, 17);
            this.SortByTitleRadioButton.TabIndex = 18;
            this.SortByTitleRadioButton.TabStop = true;
            this.SortByTitleRadioButton.Text = "Sort found entries by &title";
            this.SortByTitleRadioButton.UseVisualStyleBackColor = true;
            // 
            // hideExpiredCheckbox
            // 
            this.hideExpiredCheckbox.AutoSize = true;
            this.hideExpiredCheckbox.Location = new System.Drawing.Point(7, 88);
            this.hideExpiredCheckbox.Name = "hideExpiredCheckbox";
            this.hideExpiredCheckbox.Size = new System.Drawing.Size(152, 17);
            this.hideExpiredCheckbox.TabIndex = 17;
            this.hideExpiredCheckbox.Text = "Don\'t return e&xpired entries";
            this.hideExpiredCheckbox.UseVisualStyleBackColor = true;
            // 
            // matchSchemesCheckbox
            // 
            this.matchSchemesCheckbox.AutoSize = true;
            this.matchSchemesCheckbox.Location = new System.Drawing.Point(7, 111);
            this.matchSchemesCheckbox.Name = "matchSchemesCheckbox";
            this.matchSchemesCheckbox.Size = new System.Drawing.Size(375, 30);
            this.matchSchemesCheckbox.TabIndex = 17;
            this.matchSchemesCheckbox.Text = "&Match URL schemes\r\nonly entries with the same scheme (http://, https://, ftp://," +
    " ...) are returned";
            this.matchSchemesCheckbox.UseVisualStyleBackColor = true;
            // 
            // removePermissionsButton
            // 
            this.removePermissionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.removePermissionsButton.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.removePermissionsButton.Location = new System.Drawing.Point(14, 205);
            this.removePermissionsButton.Name = "removePermissionsButton";
            this.removePermissionsButton.Size = new System.Drawing.Size(372, 28);
            this.removePermissionsButton.TabIndex = 16;
            this.removePermissionsButton.Text = "Remo&ve all stored permissions from entries in active database";
            this.removePermissionsButton.UseVisualStyleBackColor = true;
            this.removePermissionsButton.Click += new System.EventHandler(this.removePermissionsButton_Click);
            // 
            // unlockDatabaseCheckbox
            // 
            this.unlockDatabaseCheckbox.AutoSize = true;
            this.unlockDatabaseCheckbox.Location = new System.Drawing.Point(7, 65);
            this.unlockDatabaseCheckbox.Name = "unlockDatabaseCheckbox";
            this.unlockDatabaseCheckbox.Size = new System.Drawing.Size(256, 17);
            this.unlockDatabaseCheckbox.TabIndex = 15;
            this.unlockDatabaseCheckbox.Text = "Re&quest for unlocking the database if it is locked";
            this.unlockDatabaseCheckbox.UseVisualStyleBackColor = true;
            // 
            // credMatchingCheckbox
            // 
            this.credMatchingCheckbox.AutoSize = true;
            this.credMatchingCheckbox.Location = new System.Drawing.Point(7, 29);
            this.credMatchingCheckbox.Name = "credMatchingCheckbox";
            this.credMatchingCheckbox.Size = new System.Drawing.Size(238, 30);
            this.credMatchingCheckbox.TabIndex = 9;
            this.credMatchingCheckbox.Text = "&Return only best matching entries for an URL\r\ninstead of all entries for the who" +
    "le domain";
            this.credMatchingCheckbox.UseVisualStyleBackColor = true;
            // 
            // credNotifyCheckbox
            // 
            this.credNotifyCheckbox.AutoSize = true;
            this.credNotifyCheckbox.Location = new System.Drawing.Point(7, 6);
            this.credNotifyCheckbox.Name = "credNotifyCheckbox";
            this.credNotifyCheckbox.Size = new System.Drawing.Size(267, 17);
            this.credNotifyCheckbox.TabIndex = 8;
            this.credNotifyCheckbox.Text = "Sh&ow a notification when credentials are requested";
            this.credNotifyCheckbox.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnMigrateSettings);
            this.tabPage2.Controls.Add(this.chkUseKpxcSettingsKey);
            this.tabPage2.Controls.Add(this.btnCheckForLegacyConfig);
            this.tabPage2.Controls.Add(this.chkSearchUrls);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.comboBoxDatabases);
            this.tabPage2.Controls.Add(this.txtKPXCVerOverride);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.returnStringFieldsWithKphOnlyCheckBox);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.returnStringFieldsCheckbox);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.credSearchInAllOpenedDatabases);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.credAllowUpdatesCheckbox);
            this.tabPage2.Controls.Add(this.credAllowAccessCheckbox);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(402, 461);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Advanced";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnMigrateSettings
            // 
            this.btnMigrateSettings.Location = new System.Drawing.Point(11, 417);
            this.btnMigrateSettings.Name = "btnMigrateSettings";
            this.btnMigrateSettings.Size = new System.Drawing.Size(191, 28);
            this.btnMigrateSettings.TabIndex = 39;
            this.btnMigrateSettings.Text = "Migrate Settings";
            this.btnMigrateSettings.UseVisualStyleBackColor = true;
            this.btnMigrateSettings.Click += new System.EventHandler(this.btnMigrateSettings_Click);
            // 
            // chkUseKpxcSettingsKey
            // 
            this.chkUseKpxcSettingsKey.AutoSize = true;
            this.chkUseKpxcSettingsKey.Location = new System.Drawing.Point(11, 394);
            this.chkUseKpxcSettingsKey.Name = "chkUseKpxcSettingsKey";
            this.chkUseKpxcSettingsKey.Size = new System.Drawing.Size(166, 17);
            this.chkUseKpxcSettingsKey.TabIndex = 38;
            this.chkUseKpxcSettingsKey.Text = "Use KeePassXC Settings Key";
            this.chkUseKpxcSettingsKey.UseVisualStyleBackColor = true;
            // 
            // btnCheckForLegacyConfig
            // 
            this.btnCheckForLegacyConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckForLegacyConfig.Location = new System.Drawing.Point(208, 417);
            this.btnCheckForLegacyConfig.Name = "btnCheckForLegacyConfig";
            this.btnCheckForLegacyConfig.Size = new System.Drawing.Size(186, 28);
            this.btnCheckForLegacyConfig.TabIndex = 37;
            this.btnCheckForLegacyConfig.Text = "Check for Legacy Config";
            this.btnCheckForLegacyConfig.UseVisualStyleBackColor = true;
            this.btnCheckForLegacyConfig.Click += new System.EventHandler(this.btnCheckForLegacyConfig_Click);
            // 
            // chkSearchUrls
            // 
            this.chkSearchUrls.AutoSize = true;
            this.chkSearchUrls.Location = new System.Drawing.Point(11, 134);
            this.chkSearchUrls.Name = "chkSearchUrls";
            this.chkSearchUrls.Size = new System.Drawing.Size(340, 17);
            this.chkSearchUrls.TabIndex = 36;
            this.chkSearchUrls.Text = "Search string fields for URL (matches any field starting with \"URL\")";
            this.chkSearchUrls.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 335);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(296, 26);
            this.label6.TabIndex = 35;
            this.label6.Text = "Always use this database for the KeepassNatMsg connection\r\n(leave empty to always" +
    " use the current selected database)";
            // 
            // comboBoxDatabases
            // 
            this.comboBoxDatabases.DisplayMember = "Id";
            this.comboBoxDatabases.FormattingEnabled = true;
            this.comboBoxDatabases.Location = new System.Drawing.Point(11, 364);
            this.comboBoxDatabases.Name = "comboBoxDatabases";
            this.comboBoxDatabases.Size = new System.Drawing.Size(250, 21);
            this.comboBoxDatabases.TabIndex = 34;
            this.comboBoxDatabases.ValueMember = "DbHash";
            // 
            // txtKPXCVerOverride
            // 
            this.txtKPXCVerOverride.Location = new System.Drawing.Point(161, 304);
            this.txtKPXCVerOverride.Name = "txtKPXCVerOverride";
            this.txtKPXCVerOverride.Size = new System.Drawing.Size(100, 20);
            this.txtKPXCVerOverride.TabIndex = 33;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 307);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(147, 13);
            this.label5.TabIndex = 32;
            this.label5.Text = "KeePassXC Version Override:";
            // 
            // returnStringFieldsWithKphOnlyCheckBox
            // 
            this.returnStringFieldsWithKphOnlyCheckBox.AutoSize = true;
            this.returnStringFieldsWithKphOnlyCheckBox.Location = new System.Drawing.Point(55, 230);
            this.returnStringFieldsWithKphOnlyCheckBox.Name = "returnStringFieldsWithKphOnlyCheckBox";
            this.returnStringFieldsWithKphOnlyCheckBox.Size = new System.Drawing.Size(300, 30);
            this.returnStringFieldsWithKphOnlyCheckBox.TabIndex = 31;
            this.returnStringFieldsWithKphOnlyCheckBox.Text = "Only return advanced string fields which start with \"KPH: \"\r\n(Mind the space afte" +
    "r KPH:)";
            this.returnStringFieldsWithKphOnlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(52, 263);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(277, 26);
            this.label4.TabIndex = 22;
            this.label4.Text = "Automatic creates or updates are not supported\r\nfor string fields!";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(39, 187);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(355, 46);
            this.label3.TabIndex = 21;
            this.label3.Text = "If there are more fields needed than username + password, normal \"String Fields\" " +
    "are used, which can be defined in the \"Advanced\" tab of an entry. String fields " +
    "are returned in alphabetical order.";
            // 
            // returnStringFieldsCheckbox
            // 
            this.returnStringFieldsCheckbox.AutoSize = true;
            this.returnStringFieldsCheckbox.Location = new System.Drawing.Point(11, 167);
            this.returnStringFieldsCheckbox.Name = "returnStringFieldsCheckbox";
            this.returnStringFieldsCheckbox.Size = new System.Drawing.Size(186, 17);
            this.returnStringFieldsCheckbox.TabIndex = 20;
            this.returnStringFieldsCheckbox.Text = "&Return also advanced string fields";
            this.returnStringFieldsCheckbox.UseVisualStyleBackColor = true;
            this.returnStringFieldsCheckbox.CheckedChanged += new System.EventHandler(this.returnStringFieldsCheckbox_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(52, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(299, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "Only the selected database has to be connected with a client!";
            // 
            // credSearchInAllOpenedDatabases
            // 
            this.credSearchInAllOpenedDatabases.AutoSize = true;
            this.credSearchInAllOpenedDatabases.Location = new System.Drawing.Point(11, 88);
            this.credSearchInAllOpenedDatabases.Name = "credSearchInAllOpenedDatabases";
            this.credSearchInAllOpenedDatabases.Size = new System.Drawing.Size(270, 17);
            this.credSearchInAllOpenedDatabases.TabIndex = 18;
            this.credSearchInAllOpenedDatabases.Text = "Searc&h in all opened databases for matching entries";
            this.credSearchInAllOpenedDatabases.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(4, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(391, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Activate the following options only, if you know what you are doing!";
            // 
            // credAllowUpdatesCheckbox
            // 
            this.credAllowUpdatesCheckbox.AutoSize = true;
            this.credAllowUpdatesCheckbox.Location = new System.Drawing.Point(11, 56);
            this.credAllowUpdatesCheckbox.Name = "credAllowUpdatesCheckbox";
            this.credAllowUpdatesCheckbox.Size = new System.Drawing.Size(164, 17);
            this.credAllowUpdatesCheckbox.TabIndex = 16;
            this.credAllowUpdatesCheckbox.Text = "Always allow &updating entries";
            this.credAllowUpdatesCheckbox.UseVisualStyleBackColor = true;
            // 
            // credAllowAccessCheckbox
            // 
            this.credAllowAccessCheckbox.AutoSize = true;
            this.credAllowAccessCheckbox.Location = new System.Drawing.Point(11, 33);
            this.credAllowAccessCheckbox.Name = "credAllowAccessCheckbox";
            this.credAllowAccessCheckbox.Size = new System.Drawing.Size(169, 17);
            this.credAllowAccessCheckbox.TabIndex = 15;
            this.credAllowAccessCheckbox.Text = "Always allow &access to entries";
            this.credAllowAccessCheckbox.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.scKeysMain);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(402, 461);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Keys";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // scKeysMain
            // 
            this.scKeysMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scKeysMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.scKeysMain.Location = new System.Drawing.Point(0, 0);
            this.scKeysMain.Name = "scKeysMain";
            this.scKeysMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scKeysMain.Panel1
            // 
            this.scKeysMain.Panel1.Controls.Add(this.dgvKeys);
            // 
            // scKeysMain.Panel2
            // 
            this.scKeysMain.Panel2.Controls.Add(this.btnRemoveAllKeys);
            this.scKeysMain.Panel2.Controls.Add(this.btnRemoveSelectedKeys);
            this.scKeysMain.Size = new System.Drawing.Size(402, 461);
            this.scKeysMain.SplitterDistance = 423;
            this.scKeysMain.TabIndex = 0;
            // 
            // dgvKeys
            // 
            this.dgvKeys.AllowUserToAddRows = false;
            this.dgvKeys.AllowUserToDeleteRows = false;
            this.dgvKeys.AllowUserToResizeRows = false;
            this.dgvKeys.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvKeys.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvKeys.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.colKey});
            this.dgvKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvKeys.Location = new System.Drawing.Point(0, 0);
            this.dgvKeys.Name = "dgvKeys";
            this.dgvKeys.ReadOnly = true;
            this.dgvKeys.RowHeadersVisible = false;
            this.dgvKeys.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvKeys.Size = new System.Drawing.Size(402, 423);
            this.dgvKeys.TabIndex = 0;
            // 
            // colName
            // 
            this.colName.DataPropertyName = "Name";
            this.colName.HeaderText = "Name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // colKey
            // 
            this.colKey.DataPropertyName = "Key";
            this.colKey.HeaderText = "Key";
            this.colKey.Name = "colKey";
            this.colKey.ReadOnly = true;
            // 
            // btnRemoveAllKeys
            // 
            this.btnRemoveAllKeys.Location = new System.Drawing.Point(205, 3);
            this.btnRemoveAllKeys.Name = "btnRemoveAllKeys";
            this.btnRemoveAllKeys.Size = new System.Drawing.Size(150, 23);
            this.btnRemoveAllKeys.TabIndex = 1;
            this.btnRemoveAllKeys.Text = "Remove All Keys";
            this.btnRemoveAllKeys.UseVisualStyleBackColor = true;
            this.btnRemoveAllKeys.Click += new System.EventHandler(this.btnRemoveAllKeys_Click);
            // 
            // btnRemoveSelectedKeys
            // 
            this.btnRemoveSelectedKeys.Location = new System.Drawing.Point(49, 3);
            this.btnRemoveSelectedKeys.Name = "btnRemoveSelectedKeys";
            this.btnRemoveSelectedKeys.Size = new System.Drawing.Size(150, 23);
            this.btnRemoveSelectedKeys.TabIndex = 0;
            this.btnRemoveSelectedKeys.Text = "Remove Selected Keys";
            this.btnRemoveSelectedKeys.UseVisualStyleBackColor = true;
            this.btnRemoveSelectedKeys.Click += new System.EventHandler(this.btnRemoveSelectedKeys_Click);
            // 
            // OptionsForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(411, 534);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "KeePassNatMsg Options";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.OptionsForm_Load);
            this.Shown += new System.EventHandler(this.OptionsForm_Shown);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.scKeysMain.Panel1.ResumeLayout(false);
            this.scKeysMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scKeysMain)).EndInit();
            this.scKeysMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvKeys)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.CheckBox hideExpiredCheckbox;
        private System.Windows.Forms.CheckBox matchSchemesCheckbox;
        private System.Windows.Forms.Button removePermissionsButton;
        private System.Windows.Forms.CheckBox unlockDatabaseCheckbox;
        private System.Windows.Forms.CheckBox credMatchingCheckbox;
        private System.Windows.Forms.CheckBox credNotifyCheckbox;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.CheckBox credSearchInAllOpenedDatabases;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox credAllowUpdatesCheckbox;
        private System.Windows.Forms.CheckBox credAllowAccessCheckbox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox returnStringFieldsCheckbox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton SortByUsernameRadioButton;
        private System.Windows.Forms.RadioButton SortByTitleRadioButton;
        private System.Windows.Forms.CheckBox returnStringFieldsWithKphOnlyCheckBox;
        private System.Windows.Forms.Button btnInstallNativeMessaging;
        private System.Windows.Forms.Label lblProxyVersion;
        private System.Windows.Forms.TextBox txtKPXCVerOverride;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBoxDatabases;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkSearchUrls;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.SplitContainer scKeysMain;
        private System.Windows.Forms.DataGridView dgvKeys;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKey;
        private System.Windows.Forms.Button btnRemoveAllKeys;
        private System.Windows.Forms.Button btnRemoveSelectedKeys;
        private System.Windows.Forms.Button btnCheckForLegacyConfig;
        private System.Windows.Forms.CheckBox chkUseKpxcSettingsKey;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Button btnMigrateSettings;
    }
}