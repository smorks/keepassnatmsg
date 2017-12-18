using System.Windows.Forms;

namespace KeePassHttp.NativeMessaging
{
    public partial class BrowserSelectForm : Form
    {
        public BrowserSelectForm()
        {
            InitializeComponent();
        }

        public Browsers SelectedBrowsers
        {
            get
            {
                Browsers b = Browsers.None;
                if (chkChrome.Checked) b |= Browsers.Chrome;
                if (chkChromium.Checked) b |= Browsers.Chromium;
                if (chkFirefox.Checked) b |= Browsers.Firefox;
                if (chkVivaldi.Checked) b |= Browsers.Vivaldi;
                return b;
            }
        }

        private void btnOk_Click(object sender, System.EventArgs e)
        {
            if (chkChrome.Checked || chkChromium.Checked || chkFirefox.Checked || chkVivaldi.Checked)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("You must select at least one browser to install the Native Messaging Host for.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
