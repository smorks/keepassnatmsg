﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace KeePassNatMsg.NativeMessaging
{
    public class WindowsHost : NativeMessagingHost
    {
        private const string MozillaNmhKey= "Software\\Mozilla";

        private readonly string[] RegKeys = new[] {
            string.Empty,
            "Software\\Google\\Chrome",
            "Software\\Chromium",
            MozillaNmhKey,
            "Software\\Vivaldi",
            "Software\\Microsoft\\Edge",
            MozillaNmhKey,
        };

        public override string ProxyPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeePassNatMsg");
            }
        }

        public override void Install(Browsers browsers)
        {
            var i = 0;
            foreach(Browsers b in Enum.GetValues(typeof(Browsers)))
            {
                if (b != Browsers.None && browsers.HasFlag(b))
                {
                    var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(RegKeys[i]);
                    if (key != null)
                    {
                        var nmhKey = key.CreateSubKey(string.Format("{0}\\{1}", NmhKey, GetExtKey(b)));
                        if (nmhKey != null)
                        {
                            CreateRegKeyAndFile(b, nmhKey);
                            nmhKey.Close();
                        }
                        key.Close();
                    }
                }
                i++;
            }
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
                    var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegKeys[i], false);
                    if (key != null)
                    {
                        status = BrowserStatus.Detected;
                        var nmhKey = key.OpenSubKey(string.Format("{0}\\{1}", NmhKey, GetExtKey(b)), false);
                        if (nmhKey != null)
                        {
                            var jsonFile = (string)nmhKey.GetValue(string.Empty, string.Empty);
                            if (!string.IsNullOrEmpty(jsonFile) && File.Exists(jsonFile))
                            {
                                status = BrowserStatus.Installed;
                            }
                            nmhKey.Close();
                        }
                        key.Close();
                    }
                    statuses.Add(b, status);
                }
                i++;
            }
            return statuses;
        }

        private void CreateRegKeyAndFile(Browsers b, Microsoft.Win32.RegistryKey key)
        {
            try
            {
                var jsonFile = Path.Combine(ProxyPath, string.Format("kpnm_{0}.json", b.ToString().ToLower()));
                key.SetValue(string.Empty, jsonFile, Microsoft.Win32.RegistryValueKind.String);
                if (!Directory.Exists(ProxyPath))
                {
                    Directory.CreateDirectory(ProxyPath);
                }
                File.WriteAllText(jsonFile, string.Format(GetJsonData(b), ProxyExecutable), _utf8);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ParentForm, "An error occurred attempting to install the native messaging host for KeePassNatMsg: " + ex.ToString(), "Install Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
