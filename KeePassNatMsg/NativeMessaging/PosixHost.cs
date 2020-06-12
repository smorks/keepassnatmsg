using System;
using System.Collections.Generic;
using System.IO;

namespace KeePassNatMsg.NativeMessaging
{
    public abstract class PosixHost : NativeMessagingHost
    {
        private const string PosixScript = "#!/bin/bash\nmono {0}\n";
        private const string PosixProxyPath = ".keepassnatmsg";

        private string _home = Environment.GetEnvironmentVariable("HOME");

        public override string ProxyPath => Path.Combine(_home, PosixProxyPath);

        protected abstract string[] BrowserPaths { get; }

        public override void Install(Browsers browsers)
        {
            InstallPosix(browsers);
        }

        public override Dictionary<Browsers, BrowserStatus> GetBrowserStatuses()
        {
            var statuses = new Dictionary<Browsers, BrowserStatus>();
            var i = 0;

            foreach (Browsers b in Enum.GetValues(typeof(Browsers)))
            {
                if (b != Browsers.None)
                {
                    var status = BrowserStatus.NotInstalled;
                    var jsonFile = Path.Combine(_home, BrowserPaths[i], $"{GetExtKey(b)}.json");
                    var jsonDir = Path.GetDirectoryName(jsonFile);
                    var jsonDirInfo = new DirectoryInfo(jsonDir);
                    var jsonParent = jsonDirInfo.Parent.FullName;

                    if (Directory.Exists(jsonParent))
                    {
                        status = BrowserStatus.Detected;
                    }

                    if (File.Exists(jsonFile))
                    {
                        status = BrowserStatus.Installed;
                    }
                    statuses.Add(b, status);
                }
                i++;
            }

            return statuses;
        }

        protected void InstallPosix(Browsers browsers)
        {
            if (!Directory.Exists(ProxyPath))
            {
                Directory.CreateDirectory(ProxyPath);
            }
            var monoScript = Path.Combine(ProxyPath, "run-proxy.sh");
            File.WriteAllText(monoScript, string.Format(PosixScript, ProxyExecutable), _utf8);

            Mono.Unix.Native.Syscall.stat(monoScript, out Mono.Unix.Native.Stat st);
            if (!st.st_mode.HasFlag(Mono.Unix.Native.FilePermissions.S_IXUSR))
            {
                Mono.Unix.Native.Syscall.chmod(monoScript, Mono.Unix.Native.FilePermissions.S_IXUSR | st.st_mode);
            }

            var i = 0;

            foreach (Browsers b in Enum.GetValues(typeof(Browsers)))
            {
                if (b != Browsers.None && browsers.HasFlag(b))
                {
                    var jsonFile = Path.Combine(_home, BrowserPaths[i], $"{GetExtKey(b)}.json");
                    var jsonDir = Path.GetDirectoryName(jsonFile);

                    var jsonDirInfo = new DirectoryInfo(jsonDir);
                    var jsonParent = jsonDirInfo.Parent.FullName;

                    if (Directory.Exists(jsonParent))
                    {
                        if (!Directory.Exists(jsonDir))
                        {
                            Directory.CreateDirectory(jsonDir);
                        }
                        File.WriteAllText(jsonFile, string.Format(GetJsonData(b), monoScript), _utf8);
                    }
                }
                i++;
            }
        }
    }
}
