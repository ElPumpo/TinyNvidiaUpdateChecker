using System;
using System.Runtime.InteropServices;
namespace TinyNvidiaUpdateChecker
{
    public static class ConsoleQuickEdit
    {
        const uint ENABLE_QUICK_EDIT = 0x0040;
        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        internal static bool DisableQuickEdit()
        {
            IntPtr consoleH = GetStdHandle(STD_INPUT_HANDLE);
            uint mode;
            if (!GetConsoleMode(consoleH, out mode))
            {
                return false;
            }
            mode &= ~ENABLE_QUICK_EDIT;
            if (!SetConsoleMode(consoleH, mode))
            {
                return false;
            }
            return true;
        }
    }
}
