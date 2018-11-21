using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RLBotAutoRunner
{
    public static class Keyboard
    {
        [DllImport("user32.dll")]
        private static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public static void SendKeystroke(Keys key, IntPtr window)
        {
            const uint WM_KEYDOWN = 0x100;
            const uint WM_KEYUP = 0x101;

            PostMessage(window, WM_KEYDOWN, (IntPtr)key, (IntPtr)0);
            Thread.Sleep(100);
            PostMessage(window, WM_KEYUP, (IntPtr)key, (IntPtr)0);
        }
    }
}
