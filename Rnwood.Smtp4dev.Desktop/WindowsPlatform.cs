using Chromely;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Rnwood.Smtp4dev.Desktop
{
    public class WindowsPlatform : IPlatform
    {

        public void HideWindow(IntPtr handle)
        {
            Interop.User32.ShowWindow(handle, Interop.User32.SW.HIDE);
        }

        public void ShowWindow(IntPtr handle)
        {
            Interop.User32.ShowWindow(handle, Interop.User32.SW.SHOW);
        }

        public void HideConsoleWindow()
        {
            IntPtr consoleHwnd = GetConsoleWindow();
            HideWindow(consoleHwnd);
        }

        public void ShowConsoleWindow()
        {
            IntPtr consoleHwnd = GetConsoleWindow();
            ShowWindow(consoleHwnd);
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
    }
}
