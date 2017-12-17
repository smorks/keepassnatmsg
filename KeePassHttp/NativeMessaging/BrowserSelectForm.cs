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
    }
}
