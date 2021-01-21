using KeePass.Plugins;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Utility;
using KeePassNatMsg.Utils;
using System;

namespace KeePassNatMsg.Entry
{
    public sealed class EntryTOTP
    {
        private IPluginHost _host;
        private KeePassNatMsgExt _ext;

        public EntryTOTP()
        {
            _host = KeePassNatMsgExt.HostInstance;
            _ext = KeePassNatMsgExt.ExtInstance;
        }

        public string GenerateFromUuid(string uuid)
        {
            PwEntry entry = null;
            PwUuid id = new PwUuid(MemUtil.HexStringToByteArray(uuid));

            var configOpt = new ConfigOpt(_host.CustomConfig);
            if (configOpt.SearchInAllOpenedDatabases)
            {
                foreach (PwDocument doc in _host.MainWindow.DocumentManager.Documents)
                {
                    if (doc.Database.IsOpen)
                    {
                        entry = doc.Database.RootGroup.FindEntry(id, true);
                        if (entry != null)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                entry = _host.Database.RootGroup.FindEntry(id, true);
            }

            if (entry == null)
            {
                return string.Empty;
            }

            string TotpSettings = _ext.GetTotpSettings(entry);
            if (TotpSettings == null)
            {
                return string.Empty;
            }

            try
            {
                return Totp.Generate(TotpSettings);
            }
            catch(Exception)
            {
                return string.Empty;
            }
        }
    }
}
