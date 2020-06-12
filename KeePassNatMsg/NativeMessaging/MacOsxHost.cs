namespace KeePassNatMsg.NativeMessaging
{
    public class MacOsxHost : PosixHost
    {
        private const string MozillaNmh = "Library/Application Support/Mozilla/NativeMessagingHosts";

        protected override string[] BrowserPaths => new[]
        {
            string.Empty,
            "Library/Application Support/Google/Chrome/NativeMessagingHosts",
            "Library/Application Support/Chromium/NativeMessagingHosts",
            MozillaNmh,
            "Library/Application Support/Vivaldi/NativeMessagingHosts",
            "Library/Application Support/Microsoft Edge/NativeMessagingHosts",
            MozillaNmh,
        };
    }
}
