using System.IO;

namespace KeePassHttp.NativeMessaging
{
    public sealed class NativeMessagingInstaller
    {
        public enum Browsers
        {
            Chrome,
            Chromium,
            Firefox,
            Vivaldi
        }

        public enum OperatingSystem
        {
            Windows
        }

        public void Install(OperatingSystem os, Browsers browser)
        {
            switch (os)
            {
                case OperatingSystem.Windows:
                    InstallWindows(browser);
                    break;
            }
        }

        private void InstallWindows(Browsers browser)
        {
            string nmhKey = null;
            string jsonData = null;
            switch (browser)
            {
                case Browsers.Chrome:
                    nmhKey = "Software\\Google\\Chrome\\NativeMessagingHosts\\com.varjolintu.keepassxc_browser";
                    jsonData = Properties.Resources.chrome_win;
                    break;
                case Browsers.Chromium:
                    nmhKey = "Software\\Chromium\\NativeMessagingHosts\\com.varjolintu.keepassxc_browser";
                    jsonData = Properties.Resources.chrome_win;
                    break;
                case Browsers.Firefox:
                    nmhKey = "Software\\Mozilla\\NativeMessagingHosts\\com.varjolintu.keepassxc_browser";
                    jsonData = Properties.Resources.firefox_win;
                    break;
                case Browsers.Vivaldi:
                    nmhKey = "Software\\Vivaldi\\NativeMessagingHosts\\com.varjolintu.keepassxc_browser";
                    jsonData = Properties.Resources.chrome_win;
                    break;
            }

            if (nmhKey != null)
            {
                try
                {
                    var kphAppData = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "KeePassHttp");
                    var jsonFile = Path.Combine(kphAppData, $"kph_nmh_{browser.ToString().ToLower()}.json");
                    var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(nmhKey);
                    key.SetValue(string.Empty, jsonFile, Microsoft.Win32.RegistryValueKind.String);
                    if (!Directory.Exists(kphAppData))
                    {
                        Directory.CreateDirectory(kphAppData);
                    }
                    File.WriteAllText(jsonFile, jsonData, new System.Text.UTF8Encoding(false));

                    var web = new System.Net.WebClient();
                    var latestVersion = web.DownloadString("https://github.com/smorks/keepasshttp-proxy/raw/master/version.txt");

                    System.Version v;
                    if (System.Version.TryParse(latestVersion, out v))
                    {
                        var proxyExe = Path.Combine(kphAppData, "keepasshttp-proxy.exe");
                        web.DownloadFile($"https://github.com/smorks/keepasshttp-proxy/releases/download/v{latestVersion}/keepasshttp-proxy.exe", proxyExe);
                        System.Windows.Forms.MessageBox.Show("The Native Messaging Host was installed successfully!", "Installation Complete!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    }
                }
                catch (System.Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"An error occurred during installtion: {ex}", "Installation Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
        }
    }
}
