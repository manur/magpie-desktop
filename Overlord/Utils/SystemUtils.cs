using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;

namespace Overlord.Utils
{
    static class SystemUtils
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int virtualKeyCode);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int hotKeyId);

        public const int WM_HOTKEY = 0x312;

        public const int MOD_ALT = 0x1;
        public const int MOD_CONTROL = 0x2;
        public const int MOD_SHIFT = 0x4;
        public const int MOD_WIN = 0x8;
        public const int MOD_NOREPEAT = 0x4000;

        private static int _hotkeyId = 0;

        public static void UnregisterHotkey(HotkeyWindow window, int id)
        {
            UnregisterHotKey(window.Handle, id);
        }

        public static int RegisterHotkey(HotkeyWindow window, Key key, ModifierKeys modifiers)
        {
            _hotkeyId++;

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);

            int fsModifiers = MOD_NOREPEAT;

            if ((modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                fsModifiers |= MOD_ALT;
            }

            if ((modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                fsModifiers |= MOD_CONTROL;
            }

            if ((modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                fsModifiers |= MOD_SHIFT;
            }

            if ((modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
            {
                fsModifiers |= MOD_WIN;
            }

            IntPtr hWnd = window.Handle;

            bool success = RegisterHotKey(hWnd, _hotkeyId, fsModifiers, virtualKey);

            if (!success)
            {
                return -1;
            }

            return _hotkeyId;
        }

        
    }
}
