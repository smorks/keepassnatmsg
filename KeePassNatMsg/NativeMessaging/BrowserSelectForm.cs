using System.Windows.Forms;
using KeePassNatMsg.Utils;

namespace KeePassNatMsg.NativeMessaging
{
    public partial class BrowserSelectForm : Form
    {
        private readonly NativeMessagingHost _host;

        public BrowserSelectForm(NativeMessagingHost host)
        {
            InitializeComponent();
            _host = host;
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
                if (chkMsEdge.Checked) b |= Browsers.Edge;
                if (chkThunderbird.Checked) b |= Browsers.Thunderbird;
                return b;
            }
        }

        private void btnOk_Click(object sender, System.EventArgs e)
        {
            if (chkChrome.Checked || chkChromium.Checked || chkFirefox.Checked || chkVivaldi.Checked || chkMsEdge.Checked || chkThunderbird.Checked)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("You must select at least one browser to install the Native Messaging Host for.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BrowserSelectForm_Load(object sender, System.EventArgs e)
        {
            var statuses = _host.GetBrowserStatuses();

            foreach(var b in statuses.Keys)
            {
                CheckBox cb = null;

                switch (b)
                {
                    case Browsers.Chrome:
                        cb = chkChrome;
                        break;
                    case Browsers.Chromium:
                        cb = chkChromium;
                        break;
                    case Browsers.Firefox:
                        cb = chkFirefox;
                        break;
                    case Browsers.Vivaldi:
                        cb = chkVivaldi;
                        break;
                    case Browsers.Edge:
                        cb = chkMsEdge;
                        break;
                    case Browsers.Thunderbird:
                        cb = chkThunderbird;
                        break;
                }

                if (cb != null)
                {
                    var status = statuses[b];
                    cb.Text = $"{b.GetDescription()}: {status.GetDescription()}";
                    cb.Checked = (status == BrowserStatus.Detected);
                }
            }
        }
    }
}
