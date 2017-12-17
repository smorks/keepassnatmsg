using System.Text;
using System.Windows.Forms;

namespace KeePassHttp.NativeMessaging
{
    public abstract class NativeMessagingHost
    {
        protected const string NmhKey = "NativeMessagingHosts";
        protected const string ExtKey = "com.varjolintu.keepassxc_browser";
        public const string ProxyExecutable = "keepasshttp-proxy.exe";

        protected Encoding _utf8 = new UTF8Encoding(false);

        public Form ParentForm { get; set; }

        protected string GetJsonData(Browsers b)
        {
            switch (b)
            {
                case Browsers.Chrome:
                case Browsers.Chromium:
                case Browsers.Vivaldi:
                    return Properties.Resources.chrome_win;
                case Browsers.Firefox:
                    return Properties.Resources.firefox_win;
            }
            return null;
        }

        public abstract void Install(Browsers browsers);
        public abstract string ProxyPath { get; }
    }
}
