namespace KeePassNatMsg.NativeMessaging
{
    partial class BrowserSelectForm
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
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkChrome = new System.Windows.Forms.CheckBox();
            this.chkChromium = new System.Windows.Forms.CheckBox();
            this.chkFirefox = new System.Windows.Forms.CheckBox();
            this.chkVivaldi = new System.Windows.Forms.CheckBox();
            this.chkMsEdge = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(124, 163);
            this.btnOk.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 28);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "&Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(232, 163);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // chkChrome
            // 
            this.chkChrome.AutoSize = true;
            this.chkChrome.Location = new System.Drawing.Point(16, 15);
            this.chkChrome.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkChrome.Name = "chkChrome";
            this.chkChrome.Size = new System.Drawing.Size(129, 21);
            this.chkChrome.TabIndex = 3;
            this.chkChrome.Text = "Google Chrome";
            this.chkChrome.UseVisualStyleBackColor = true;
            // 
            // chkChromium
            // 
            this.chkChromium.AutoSize = true;
            this.chkChromium.Location = new System.Drawing.Point(16, 43);
            this.chkChromium.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkChromium.Name = "chkChromium";
            this.chkChromium.Size = new System.Drawing.Size(93, 21);
            this.chkChromium.TabIndex = 4;
            this.chkChromium.Text = "Chromium";
            this.chkChromium.UseVisualStyleBackColor = true;
            // 
            // chkFirefox
            // 
            this.chkFirefox.AutoSize = true;
            this.chkFirefox.Location = new System.Drawing.Point(16, 71);
            this.chkFirefox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkFirefox.Name = "chkFirefox";
            this.chkFirefox.Size = new System.Drawing.Size(119, 21);
            this.chkFirefox.TabIndex = 5;
            this.chkFirefox.Text = "Mozilla Firefox";
            this.chkFirefox.UseVisualStyleBackColor = true;
            // 
            // chkVivaldi
            // 
            this.chkVivaldi.AutoSize = true;
            this.chkVivaldi.Location = new System.Drawing.Point(16, 100);
            this.chkVivaldi.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkVivaldi.Name = "chkVivaldi";
            this.chkVivaldi.Size = new System.Drawing.Size(71, 21);
            this.chkVivaldi.TabIndex = 6;
            this.chkVivaldi.Text = "Vivaldi";
            this.chkVivaldi.UseVisualStyleBackColor = true;
            // 
            // chkMsEdge
            // 
            this.chkMsEdge.AutoSize = true;
            this.chkMsEdge.Location = new System.Drawing.Point(16, 129);
            this.chkMsEdge.Margin = new System.Windows.Forms.Padding(4);
            this.chkMsEdge.Name = "chkMsEdge";
            this.chkMsEdge.Size = new System.Drawing.Size(124, 21);
            this.chkMsEdge.TabIndex = 7;
            this.chkMsEdge.Text = "Microsoft Edge";
            this.chkMsEdge.UseVisualStyleBackColor = true;
            // 
            // BrowserSelectForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(348, 206);
            this.Controls.Add(this.chkMsEdge);
            this.Controls.Add(this.chkVivaldi);
            this.Controls.Add(this.chkFirefox);
            this.Controls.Add(this.chkChromium);
            this.Controls.Add(this.chkChrome);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "BrowserSelectForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Browsers";
            this.Load += new System.EventHandler(this.BrowserSelectForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkChrome;
        private System.Windows.Forms.CheckBox chkChromium;
        private System.Windows.Forms.CheckBox chkFirefox;
        private System.Windows.Forms.CheckBox chkVivaldi;
        private System.Windows.Forms.CheckBox chkMsEdge;
    }
}