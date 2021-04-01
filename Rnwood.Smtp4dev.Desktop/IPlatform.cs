using System;

namespace Rnwood.Smtp4dev.Desktop
{
    public interface IPlatform
    {
        void HideConsoleWindow();

        void ShowConsoleWindow();

        void HideWindow(IntPtr handle);
        void ShowWindow(IntPtr handle);
    }
}