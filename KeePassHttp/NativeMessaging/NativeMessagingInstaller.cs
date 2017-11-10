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
            switch (browser)
            {
                case Browsers.Chrome:
                    nmhKey = "Software\\Google\\Chrome\\NativeMessagingHosts\\com.varjolintu.keepassxc_browser";
                    break;
                case Browsers.Chromium:
                    nmhKey = "Software\\Chromium\\NativeMessagingHosts\\com.varjolintu.keepassxc_browser";
                    break;
                case Browsers.Firefox:
                    nmhKey = "Software\\Mozilla\\NativeMessagingHosts\\com.varjolintu.keepassxc_browser";
                    break;
                case Browsers.Vivaldi:
                    nmhKey = "Software\\Vivaldi\\NativeMessagingHosts\\com.varjolintu.keepassxc_browser";
                    break;
            }

            if (nmhKey != null)
            {
                var jsonFile = string.Empty;
                var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(nmhKey);
                key.SetValue(string.Empty, jsonFile, Microsoft.Win32.RegistryValueKind.String);
            }
        }
    }
}
