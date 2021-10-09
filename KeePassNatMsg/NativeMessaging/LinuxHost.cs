namespace KeePassNatMsg.NativeMessaging
{
    public class LinuxHost : PosixHost
    {
        private const string MozillaNmh = ".mozilla/native-messaging-hosts";

        protected override string[] BrowserPaths
        {
            get
            {
                return new[]
                {
                    string.Empty,
                    ".config/google-chrome/NativeMessagingHosts",
                    ".config/chromium/NativeMessagingHosts",
                    MozillaNmh,
                    ".config/vivaldi/NativeMessagingHosts",
                    ".config/microsoft edge/NativeMessagingHosts",
                    MozillaNmh,
                };
            }
        }
    }
}
