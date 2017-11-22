using System;
using System.IO;

namespace KeePassHttp.NativeMessaging
{
    public sealed class NativeMessagingInstaller
    {
        public enum Browsers
        {
            Chrome = 1,
            Chromium = 2,
            Firefox = 4,
            Vivaldi = 8
        }

        public enum OperatingSystem
        {
            Windows
        }

        private const string NmhKey = "NativeMessagingHosts";
        private const string ExtKey = "com.varjolintu.keepassxc_browser";

        private string KphAppData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeePassHttp");

        public void Install(OperatingSystem os)
        {
            switch (os)
            {
                case OperatingSystem.Windows:
                    InstallWindows();
                    break;
            }
        }

        private void InstallWindows()
        {
            int browsers = 0;

            var keys = new[] { "Software\\Google\\Chrome", "Software\\Chromium", "Software\\Mozilla", "Software\\Vivaldi" };
            var browserMap = new[] { Browsers.Chrome, Browsers.Chromium, Browsers.Firefox, Browsers.Vivaldi };

            for (var i=0;i<keys.Length;i++)
            {
                var key = keys[i];
                var b = browserMap[i];
                var bkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key, false);
                if (bkey != null)
                {
                    var nmhKey = bkey.CreateSubKey($"{NmhKey}\\{ExtKey}");
                    if (nmhKey != null)
                    {
                        CreateRegKeyAndFile(b, nmhKey);
                        browsers += (int)b;
                    }
                }
            }

            if (DownloadProxy())
            {
                System.Windows.Forms.MessageBox.Show("The Native Messaging Host was installed successfully!", "Installation Complete!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
        }

        private string GetJsonData(Browsers b)
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

        private void CreateRegKeyAndFile(Browsers b, Microsoft.Win32.RegistryKey key)
        {
            try
            {
                var jsonFile = Path.Combine(KphAppData, $"kph_nmh_{b.ToString().ToLower()}.json");
                key.SetValue(string.Empty, jsonFile, Microsoft.Win32.RegistryValueKind.String);
                if (!Directory.Exists(KphAppData))
                {
                    Directory.CreateDirectory(KphAppData);
                }
                File.WriteAllText(jsonFile, GetJsonData(b), new System.Text.UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"An error occurred attempting to install the native messaging host for KeePassHttp: {ex}", "Install Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private bool DownloadProxy()
        {
            try
            {
                var web = new System.Net.WebClient();
                var latestVersion = web.DownloadString("https://github.com/smorks/keepasshttp-proxy/raw/master/version.txt");

                if (Version.TryParse(latestVersion, out Version lv))
                {
                    var proxyExe = Path.Combine(KphAppData, "keepasshttp-proxy.exe");
                    var newVersion = false;

                    if (File.Exists(proxyExe))
                    {
                        var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(proxyExe);
                        if (Version.TryParse(fvi.FileVersion, out Version exeVer))
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
                        web.DownloadFile($"https://github.com/smorks/keepasshttp-proxy/releases/download/v{latestVersion}/keepasshttp-proxy.exe", proxyExe);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"An error occurred attempting to download the proxy application: {ex}", "Proxy Download Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return false;
        }
    }
}
