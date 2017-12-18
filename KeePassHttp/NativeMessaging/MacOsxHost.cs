namespace KeePassHttp.NativeMessaging
{
    public class MacOsxHost : PosixHost
    {
        protected override string[] BrowserPaths => new[]
        {
            string.Empty,
            "Library/Application Support/Google/Chrome/NativeMessagingHosts",
            "Library/Application Support/Chromium/NativeMessagingHosts",
            "Library/Application Support/Mozilla/NativeMessagingHosts",
            "Library/Application Support/Vivaldi/NativeMessagingHosts"
        };
    }
}
