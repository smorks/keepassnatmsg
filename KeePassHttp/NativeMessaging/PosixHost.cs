using System;
using System.IO;

namespace KeePassHttp.NativeMessaging
{
    public abstract class PosixHost : NativeMessagingHost
    {
        private const string PosixScript = "#!/bin/bash\nmono {0}\n";
        private const string PosixProxyPath = ".keepasshttp";

        private string _home = Environment.GetEnvironmentVariable("HOME");

        public override string ProxyPath => Path.Combine(_home, PosixProxyPath);

        protected abstract string[] BrowserPaths { get; }

        public override void Install(Browsers browsers)
        {
            InstallPosix(browsers);
        }

        protected void InstallPosix(Browsers browsers)
        {
            var proxyPath = Path.Combine(_home, PosixProxyPath);
            if (!Directory.Exists(proxyPath))
            {
                Directory.CreateDirectory(proxyPath);
            }
            var monoScript = Path.Combine(proxyPath, "run-proxy.sh");
            File.WriteAllText(monoScript, string.Format(PosixScript, ProxyExecutable), _utf8);

            Mono.Unix.Native.Syscall.stat(monoScript, out Mono.Unix.Native.Stat st);
            if (!st.st_mode.HasFlag(Mono.Unix.Native.FilePermissions.S_IXUSR))
            {
                Mono.Unix.Native.Syscall.chmod(monoScript, Mono.Unix.Native.FilePermissions.S_IXUSR | st.st_mode);
            }

            var i = 0;

            foreach (Browsers b in Enum.GetValues(typeof(Browsers)))
            {
                if (browsers.HasFlag(b))
                {
                    var jsonFile = Path.Combine(_home, BrowserPaths[i], $"{ExtKey}.json");
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
