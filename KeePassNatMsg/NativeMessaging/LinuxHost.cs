namespace KeePassNatMsg.NativeMessaging
{
    public class LinuxHost : PosixHost
    {
        protected override string[] BrowserPaths => new[]
        {
            string.Empty,
            ".config/google-chrome/NativeMessagingHosts",
            ".config/chromium/NativeMessagingHosts",
            ".mozilla/native-messaging-hosts",
            ".config/vivaldi/NativeMessagingHosts",
            ".config/microsoft edge/NativeMessagingHosts",
        };
    }
}
