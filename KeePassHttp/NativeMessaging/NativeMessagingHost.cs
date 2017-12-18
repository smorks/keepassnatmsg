using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace KeePassHttp.NativeMessaging
{
    public abstract class NativeMessagingHost
    {
        protected const string NmhKey = "NativeMessagingHosts";
        protected const string ExtKey = "com.varjolintu.keepassxc_browser";
        protected const string ProxyExecutable = "keepasshttp-proxy.exe";
        private const string GithubRepo = "https://github.com/smorks/keepasshttp-proxy";

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

        public Version GetProxyVersion()
        {
            if (File.Exists(ProxyPath))
            {
                var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(ProxyPath);
                if (Version.TryParse(fvi.FileVersion, out Version exeVer))
                {
                    return exeVer;
                }
            }
            return null;
        }

        public bool UpdateProxy()
        {
            try
            {
                var web = new System.Net.WebClient();
                var latestVersion = web.DownloadString($"{GithubRepo}/raw/master/version.txt");

                if (Version.TryParse(latestVersion, out Version lv))
                {
                    var exeVer = GetProxyVersion();
                    var newVersion = exeVer == null ? true : lv > exeVer;

                    if (newVersion)
                    {
                        web.DownloadFile($"{GithubRepo}/releases/download/v{latestVersion}/{ProxyExecutable}", ProxyPath);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ParentForm, $"An error occurred attempting to download the proxy application: {ex}", "Proxy Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        public abstract string ProxyPath { get; }
        public abstract void Install(Browsers browsers);
    }
}
