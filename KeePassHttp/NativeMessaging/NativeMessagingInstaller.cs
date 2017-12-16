using System;
using System.Collections.Generic;
using System.IO;

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

        private const string NmhKey = "NativeMessagingHosts";
        private const string ExtKey = "com.varjolintu.keepassxc_browser";
        private const string ProxyExecutable = "keepasshttp-proxy.exe";
        private const string GithubRepo = "https://github.com/smorks/keepasshttp-proxy";
        private const string LinuxScript = "#!/bin/bash\nmono {0}\n";
        private const string LinuxProxyPath = ".keepasshttp";

        private System.Text.Encoding _utf8 = new System.Text.UTF8Encoding(false);

        private string KphAppData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeePassHttp");

        public void Install()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    InstallWindows();
                    break;
                case PlatformID.Unix:
                    InstallLinux();
                    break;
                case PlatformID.MacOSX:
                    InstallMaxOsx();
                    break;
            }
        }

        #region "Windows"

        private void InstallWindows()
        {
            var browsers = new List<string>();
            var keys = new[] { "Software\\Google\\Chrome", "Software\\Chromium", "Software\\Mozilla", "Software\\Vivaldi" };

            for (var i=0;i<keys.Length;i++)
            {
                var key = keys[i];
                var b = (Browsers)i;
                var bkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key, true);
                if (bkey != null)
                {
                    var nmhKey = bkey.CreateSubKey($"{NmhKey}\\{ExtKey}");
                    if (nmhKey != null)
                    {
                        CreateRegKeyAndFile(b, nmhKey);
                        browsers.Add(b.ToString());
                        nmhKey.Close();
                    }
                    bkey.Close();
                }
            }

            var msg = new System.Text.StringBuilder();

            if (browsers.Count > 0)
            {
                msg.Append("\n\nRegistry Keys and Files were created for the following browsers:\n");
                msg.Append(string.Join("\n", browsers));
            }

            var proxyExe = Path.Combine(KphAppData, ProxyExecutable);
            if (DownloadProxy(msg, proxyExe))
            {
                System.Windows.Forms.MessageBox.Show($"The Native Messaging Host was installed successfully!{msg}", "Installation Complete!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
        }

        private string GetJsonData(Browsers b)
        {
            switch (b)
            {
                case Browsers.Chrome:
                case Browsers.Chromium:
                case Browsers.Vivaldi:
                    return Properties.Resources.chrome_win;
                case Browsers.Firefox:
                    return Properties.Resources.firefox_win;
            }
            return null;
        }

        private void CreateRegKeyAndFile(Browsers b, Microsoft.Win32.RegistryKey key)
        {
            try
            {
                var jsonFile = Path.Combine(KphAppData, $"kph_nmh_{b.ToString().ToLower()}.json");
                key.SetValue(string.Empty, jsonFile, Microsoft.Win32.RegistryValueKind.String);
                if (!Directory.Exists(KphAppData))
                {
                    Directory.CreateDirectory(KphAppData);
                }
                File.WriteAllText(jsonFile, string.Format(GetJsonData(b), ProxyExecutable), _utf8);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"An error occurred attempting to install the native messaging host for KeePassHttp: {ex}", "Install Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        #endregion

        #region "Linux"

        private void InstallLinux()
        {
            InstallPosix(new[]
            {
                ".config/google-chrome/NativeMessagingHosts",
                ".config/chromium/NativeMessagingHosts",
                ".mozilla/native-messaging-hosts",
                ".config/vivaldi/NativeMessagingHosts"
            });
        }

        #endregion

        #region "Mac OSX"

        private void InstallMaxOsx()
        {
            InstallPosix(new[]
            {
                "Library/Application Support/Google/Chrome/NativeMessagingHosts",
                "Library/Application Support/Chromium/NativeMessagingHosts",
                "Library/Application Support/Mozilla/NativeMessagingHosts",
                "Library/Application Support/Vivaldi/NativeMessagingHosts"
            });
        }

        #endregion

        private void InstallPosix(string[] browserPaths)
        {
            var home = Environment.GetEnvironmentVariable("HOME");

            var proxyPath = Path.Combine(home, LinuxProxyPath);
            if (!Directory.Exists(proxyPath))
            {
                Directory.CreateDirectory(proxyPath);
            }
            var monoScript = Path.Combine(proxyPath, "run-proxy.sh");
            File.WriteAllText(monoScript, string.Format(LinuxScript, ProxyExecutable), _utf8);

            Mono.Unix.Native.Syscall.stat(monoScript, out Mono.Unix.Native.Stat st);
            if (!st.st_mode.HasFlag(Mono.Unix.Native.FilePermissions.S_IXUSR))
            {
                Mono.Unix.Native.Syscall.chmod(monoScript, Mono.Unix.Native.FilePermissions.S_IXUSR | st.st_mode);
            }

            var browsers = new List<string>();

            for (var i = 0; i < browserPaths.Length; i++)
            {
                var jsonFile = Path.Combine(home, browserPaths[i], $"{ExtKey}.json");
                var jsonDir = Path.GetDirectoryName(jsonFile);
                var b = (Browsers)i;

                var jsonDirInfo = new DirectoryInfo(jsonDir);
                var jsonParent = jsonDirInfo.Parent.FullName;

                if (Directory.Exists(jsonParent))
                {
                    browsers.Add(b.ToString());
                    if (!Directory.Exists(jsonDir))
                    {
                        Directory.CreateDirectory(jsonDir);
                    }
                    File.WriteAllText(jsonFile, string.Format(GetJsonData(b), monoScript), _utf8);
                }
            }

            var msg = new System.Text.StringBuilder();

            if (browsers.Count > 0)
            {
                msg.Append("\n\nNative Messaging Files were created for the following browsers:\n");
                msg.Append(string.Join("\n", browsers));
            }

            var proxyExe = Path.Combine(proxyPath, ProxyExecutable);
            if (DownloadProxy(msg, proxyExe))
            {
                System.Windows.Forms.MessageBox.Show($"The Native Messaging Host was installed successfully!{msg}", "Installation Complete!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
        }

        private bool DownloadProxy(System.Text.StringBuilder msg, string proxyExePath)
        {
            try
            {
                var web = new System.Net.WebClient();
                var latestVersion = web.DownloadString($"{GithubRepo}/raw/master/version.txt");
                Version lv;

                if (Version.TryParse(latestVersion, out lv))
                {
                    var newVersion = false;

                    if (File.Exists(proxyExePath))
                    {
                        var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(proxyExePath);
                        Version exeVer;
                        if (Version.TryParse(fvi.FileVersion, out exeVer))
                        {
                            newVersion = lv > exeVer;
                        }
                    }
                    else
                    {
                        newVersion = true;
                    }

                    if (newVersion)
                    {
                        web.DownloadFile($"{GithubRepo}/releases/download/v{latestVersion}/{ProxyExecutable}", proxyExePath);
                        msg.Append($"\n\nProxy updated to version {lv}");
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"An error occurred attempting to download the proxy application: {ex}", "Proxy Download Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return false;
        }
    }
}
