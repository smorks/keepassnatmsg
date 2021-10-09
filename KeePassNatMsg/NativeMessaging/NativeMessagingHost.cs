using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace KeePassNatMsg.NativeMessaging
{
    [Flags]
    public enum Browsers
    {
        None,

        [Description("Google Chrome")]
        Chrome,

        Chromium,
        Firefox = 4,
        Vivaldi = 8,

        [Description("Microsoft Edge")]
        Edge = 16,
        Thunderbird = 32,
    }

    public enum BrowserStatus
    {
        [Description("Not Installed")]
        NotInstalled,
        Detected,
        Installed
    }

    public abstract class NativeMessagingHost
    {
        protected const string NmhKey = "NativeMessagingHosts";
        protected const string ProxyExecutable = "keepassnatmsg-proxy.exe";
        private const string GithubRepo = "https://github.com/smorks/keepassnatmsg-proxy";
        private const string ExtKeyBrowser = "org.keepassxc.keepassxc_browser";
        private const string ExtKeyThunderbird = "de.kkapsner.keepassxc_mail";

        protected Encoding _utf8 = new UTF8Encoding(false);

        public Form ParentForm { get; set; }
        public string ProxyExePath
        {
            get
            {
                return Path.Combine(ProxyPath, ProxyExecutable);
            }
        }

        protected NativeMessagingHost()
        {
            // enable TLS 1.2
            System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
        }

        public static NativeMessagingHost GetHost()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return new WindowsHost();
                case PlatformID.Unix:
                    return new LinuxHost();
                case PlatformID.MacOSX:
                    return new MacOsxHost();
                default:
                    throw new PlatformNotSupportedException(string.Format("{0} is not a supported platform.", Environment.OSVersion.Platform));
            }
        }

        protected string GetJsonData(Browsers b)
        {
            switch (b)
            {
                case Browsers.Chrome:
                case Browsers.Chromium:
                case Browsers.Vivaldi:
                    return Properties.Resources.chrome_json;
                case Browsers.Firefox:
                    return Properties.Resources.firefox_json;
                case Browsers.Edge:
                    return Properties.Resources.edge_json;
                case Browsers.Thunderbird:
                    return Properties.Resources.thunderbird_json;
            }
            return null;
        }

        public Version GetProxyVersion()
        {
            if (File.Exists(ProxyExePath))
            {
                var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(ProxyExePath);
                Version exeVer;
                if (Version.TryParse(fvi.FileVersion, out exeVer))
                {
                    return exeVer;
                }
            }
            return null;
        }

        public Version GetLatestProxyVersion()
        {
            var web = new System.Net.WebClient();
            var latestVersion = web.DownloadString(string.Format("{0}/raw/master/version.txt", GithubRepo));
            Version lv;
            if (Version.TryParse(latestVersion, out lv))
            {
                return lv;
            }
            return null;
        }

        public bool UpdateProxy()
        {
            try
            {
                var latestVersion = GetLatestProxyVersion();
                var exeVer = GetProxyVersion();
                var newVersion = exeVer == null ? true : latestVersion > exeVer;

                if (newVersion)
                {
                    var web = new System.Net.WebClient();
                    web.DownloadFile(string.Format("{0}/releases/download/v{1}/{2}", GithubRepo, latestVersion, ProxyExecutable), ProxyExePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ParentForm, "An error occurred attempting to download the proxy application: " + ex.ToString(), "Proxy Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        protected string GetExtKey(Browsers b)
        {
            switch(b)
            {
                case Browsers.Thunderbird:
                    return ExtKeyThunderbird;
                default:
                    return ExtKeyBrowser;
            }
        }

        public abstract string ProxyPath { get; }
        public abstract void Install(Browsers browsers);
        public abstract Dictionary<Browsers, BrowserStatus> GetBrowserStatuses();
    }
}
