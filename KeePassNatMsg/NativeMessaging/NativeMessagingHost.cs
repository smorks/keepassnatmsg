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
        Vivaldi = 8
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
        protected const string ExtKey = "org.keepassxc.keepassxc_browser";
        protected const string ProxyExecutable = "keepassnatmsg-proxy.exe";
        private const string GithubRepo = "https://github.com/smorks/keepassnatmsg-proxy";

        protected Encoding _utf8 = new UTF8Encoding(false);

        public Form ParentForm { get; set; }
        public string ProxyExePath => Path.Combine(ProxyPath, ProxyExecutable);

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
                    throw new PlatformNotSupportedException($"{Environment.OSVersion.Platform} is not a supported platform.");
            }
        }

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
            if (File.Exists(ProxyExePath))
            {
                var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(ProxyExePath);
                if (Version.TryParse(fvi.FileVersion, out Version exeVer))
                {
                    return exeVer;
                }
            }
            return null;
        }

        public Version GetLatestProxyVersion()
        {
            var web = new System.Net.WebClient();
            var latestVersion = web.DownloadString($"{GithubRepo}/raw/master/version.txt");

            if (Version.TryParse(latestVersion, out Version lv))
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
                    web.DownloadFile($"{GithubRepo}/releases/download/v{latestVersion}/{ProxyExecutable}", ProxyExePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ParentForm, $"An error occurred attempting to download the proxy application: {ex}", "Proxy Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        public abstract string ProxyPath { get; }
        public abstract void Install(Browsers browsers);
        public abstract Dictionary<Browsers, BrowserStatus> GetBrowserStatuses();
    }
}
