namespace KeePassHttp.NativeMessaging
{
    public class LinuxHost : PosixHost
    {
        protected override string[] BrowserPaths => new[]
        {
            ".config/google-chrome/NativeMessagingHosts",
            ".config/chromium/NativeMessagingHosts",
            ".mozilla/native-messaging-hosts",
            ".config/vivaldi/NativeMessagingHosts"
        };
    }
}
