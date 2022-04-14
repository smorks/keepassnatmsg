using System.Windows.Forms;
using KeePass.Forms;

namespace KeePassNatMsg.Ui
{
    public class PwEntryFormExt
    {
        private const string TabTitle = "Browser Integration";
        private const string TabControlName = "m_tabMain";
        private readonly PwEntryForm _form;
        private readonly TabPage _page;

        public PwEntryFormExt(PwEntryForm form)
        {
            _form = form;

            var tc = FindTabControl();
            if (tc == null) return;

            _page = new TabPage(TabTitle);
            var idx = tc.TabPages.Count - 1;
            tc.TabPages.Insert(idx, _page);
        }

        private TabControl FindTabControl()
        {
            var ctrls = _form.Controls.Find(TabControlName, true);

            if (ctrls.Length == 1)
            {
                return ctrls[0] as TabControl;
            }

            return null;
        }
    }
}