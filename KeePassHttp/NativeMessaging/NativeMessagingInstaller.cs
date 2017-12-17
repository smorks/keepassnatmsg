using System;
using System.IO;
using System.Windows.Forms;

namespace KeePassHttp.NativeMessaging
{
    [Flags]
    public enum Browsers
    {
        None,
        Chrome,
        Chromium,
        Firefox = 4,
        Vivaldi = 8
    }

    public sealed class NativeMessagingInstaller
    {
        private const string GithubRepo = "https://github.com/smorks/keepasshttp-proxy";

        private readonly Form _form;
        private readonly NativeMessagingHost _host;

        public NativeMessagingInstaller(Form form)
        {
            _form = form;

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    _host = new WindowsHost();
                    break;
                case PlatformID.Unix:
                    _host = new LinuxHost();
                    break;
                case PlatformID.MacOSX:
                    _host = new MacOsxHost();
                    break;
            }

            _host.ParentForm = form;
        }

        public bool IsInstalled()
        {
            return false;
        }

        public void Install(Browsers browsers)
        {
            _host.Install(browsers);
        }

        private bool DownloadProxy(System.Text.StringBuilder msg, string proxyExePath)
        {
            try
            {
                var web = new System.Net.WebClient();
                var latestVersion = web.DownloadString($"{GithubRepo}/raw/master/version.txt");
                Version lv;

                if (Version.TryParse(latestVersion, out lv))
                {
                    var newVersion = false;

                    if (File.Exists(proxyExePath))
                    {
                        var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(proxyExePath);
                        Version exeVer;
                        if (Version.TryParse(fvi.FileVersion, out exeVer))
                        {
                            newVersion = lv > exeVer;
                        }
                    }
                    else
                    {
                        newVersion = true;
                    }

                    if (newVersion)
                    {
                        web.DownloadFile($"{GithubRepo}/releases/download/v{latestVersion}/{NativeMessagingHost.ProxyExecutable}", proxyExePath);
                        msg.Append($"\n\nProxy updated to version {lv}");
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_form, $"An error occurred attempting to download the proxy application: {ex}", "Proxy Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }
    }
}
