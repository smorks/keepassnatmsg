using System;
using System.IO;
using System.Windows.Forms;

namespace KeePassHttp.NativeMessaging
{
    public class WindowsHost : NativeMessagingHost
    {
        private string KphAppData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeePassHttp");
        private string[] RegKeys = new[] { "Software\\Google\\Chrome", "Software\\Chromium", "Software\\Mozilla", "Software\\Vivaldi" };

        public override string ProxyPath => Path.Combine(KphAppData, ProxyExecutable);

        public override void Install(Browsers browsers)
        {
            var i = 0;
            foreach(Browsers b in Enum.GetValues(typeof(Browsers)))
            {
                if (b != Browsers.None && browsers.HasFlag(b))
                {
                    var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RegKeys[i], true);
                    if (key != null)
                    {
                        var nmhKey = key.CreateSubKey($"{NmhKey}\\{ExtKey}");
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
                MessageBox.Show(ParentForm, $"An error occurred attempting to install the native messaging host for KeePassHttp: {ex}", "Install Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
