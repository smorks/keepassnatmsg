using System;
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
            _host.UpdateProxy();
        }

        public Version GetProxyVersion() => _host.GetProxyVersion();
    }
}
