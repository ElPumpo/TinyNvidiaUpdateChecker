using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TinyNvidiaUpdateChecker.Handlers
{
    class GenericHandler
    {
        const uint ENABLE_QUICK_EDIT = 0x0040;
        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        /// <summary>
        /// Disable QuickEdit, highlighting something in the window will stall the application
        /// https://stackoverflow.com/a/36720802
        /// </summary>
        public static void DisableQuickEdit()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
            uint consoleMode;

            if (!GetConsoleMode(consoleHandle, out consoleMode)) {
                return;
            }

            consoleMode &= ~ENABLE_QUICK_EDIT;

            if (!SetConsoleMode(consoleHandle, consoleMode)){
                return;
            }
        }

        public static string[] CheckPathForInstaller(string path)
        {
            string[] Installers;
            Installers = Directory.GetFiles(path, "*-international-whql.exe");
            return Installers;
        }
    }
}
