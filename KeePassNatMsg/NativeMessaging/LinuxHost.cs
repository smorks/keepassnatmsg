namespace KeePassNatMsg.NativeMessaging
{
    public class LinuxHost : PosixHost
    {
        private const string MozillaNmh = ".mozilla/native-messaging-hosts";

        protected override string[] BrowserPaths => new[]
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
