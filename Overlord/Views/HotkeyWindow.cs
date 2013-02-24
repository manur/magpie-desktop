using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Overlord
{
    class HotkeyWindow: Form
    {
        public HotkeyWindow():
            base()
        {
            this.Visible = false;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Utils.SystemUtils.WM_HOTKEY)
            {
                App.Controller.OnHotkey();
            }

            base.WndProc(ref m);
        }
    }
}
