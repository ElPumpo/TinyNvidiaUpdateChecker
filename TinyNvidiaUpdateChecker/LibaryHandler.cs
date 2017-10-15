using Microsoft.Win32;
using System;
using System.IO;

namespace TinyNvidiaUpdateChecker
{
    class LibaryHandler
    {

        private static bool is64 = Environment.Is64BitOperatingSystem;

        public enum Libary
        {
            SEVENZIP,
            WINRAR
        }

        public static LibaryFile EvaluateLibary() {
            LibaryFile WinRAR = CheckWinRAR(); // CheckWinRAR
            LibaryFile SevenZip = Check7Zip(); // Check7Zip

            if (WinRAR.IsInstalled()) {
                return WinRAR;
            } else if (SevenZip.IsInstalled()) {
                return SevenZip;
            } else {
                return null;
            }
        }

        private static LibaryFile CheckWinRAR() {
            try {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\WinRAR archiver", false)) {
                    LogManager.Log("WinRAR path: " + regKey.GetValue("InstallLocation").ToString(), LogManager.Level.INFO);
                    return new LibaryFile(regKey.GetValue("InstallLocation").ToString(), Libary.WINRAR, true);
                }
            } catch (Exception) { }

            try {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\WinRAR archiver", false)) {
                    LogManager.Log("WinRAR path: " + regKey.GetValue("InstallLocation").ToString(), LogManager.Level.INFO);
                    return new LibaryFile(regKey.GetValue("InstallLocation").ToString(), Libary.WINRAR, true);
                }
            }
            catch (Exception) { }

            return new LibaryFile(false);
        }

        private static LibaryFile Check7Zip() {
            /* Debug directory */
            if(Directory.Exists("7-Zip")) {
                LogManager.Log("7-Zip path: " + Path.GetFullPath("7-Zip") + @"\", LogManager.Level.INFO);
                return new LibaryFile(Path.GetFullPath("7-Zip") + @"\", Libary.SEVENZIP, true);
            }

            /* Default installer */

            // amd64 installer on amd64 system, or x86 on x86 system
            try
            {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip", false)) {
                    LogManager.Log("7-Zip path: " + regKey.GetValue("InstallLocation").ToString(), LogManager.Level.INFO);
                    return new LibaryFile(regKey.GetValue("InstallLocation").ToString(), Libary.SEVENZIP, true);
                }
            }
            catch (Exception) { }

            // x86 intaller on amd64 system
            if (is64) {
                try {
                    using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip", false)) {
                        LogManager.Log("7-Zip path: " + regKey.GetValue("InstallLocation").ToString(), LogManager.Level.INFO);
                        return new LibaryFile(regKey.GetValue("InstallLocation").ToString(), Libary.SEVENZIP, true);
                    }
                }
                catch (Exception) { }
            }

            /* MSI installer */
            
            // amd64 installer on amd64 system, or x86 on x86 system
            try {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\7-Zip", false)) {
                    LogManager.Log("7-Zip path: " + regKey.GetValue("Path").ToString(), LogManager.Level.INFO);
                    return new LibaryFile(regKey.GetValue("Path").ToString(), Libary.SEVENZIP, true);
                }
            }
            catch (Exception) { }

            // x86 intaller on amd64 system
            if (is64) {
                try {
                    using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\7-Zip", false)) {
                        LogManager.Log("7-Zip path: " + regKey.GetValue("Path").ToString(), LogManager.Level.INFO);
                        return new LibaryFile(regKey.GetValue("Path").ToString(), Libary.SEVENZIP, true);
                    }
                }
                catch (Exception) { }
            }

            /* Scoop support */
            string path;
            
            // installed in user profile
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "scoop", "apps", "7zip", "current");
            if (Directory.Exists(path)) {
                path += @"\";
                LogManager.Log("7-Zip path: " + path, LogManager.Level.INFO);
                return new LibaryFile(path, Libary.SEVENZIP, true);
            }

            // installed in Program Data
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "scoop", "apps", "7zip", "current");
            if (Directory.Exists(path)) {
                path += @"\";
                LogManager.Log("7-Zip path: " + path, LogManager.Level.INFO);
                return new LibaryFile(path, Libary.SEVENZIP, true);
            }

            /* Last resort checks */

            // amd64 on amd64 system, or x86 on x86 system
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip");
            if (Directory.Exists(path)) {
                path += @"\";
                LogManager.Log("7-Zip path: " + path, LogManager.Level.INFO);
                return new LibaryFile(path, Libary.SEVENZIP, true);
            }

            // x86 on amd64 system
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip");
            if (Directory.Exists(path)) {
                path += @"\";
                LogManager.Log("7-Zip path: " + path, LogManager.Level.INFO);
                return new LibaryFile(path, Libary.SEVENZIP, true);
            }

            return new LibaryFile(false);
        }
    }

    class LibaryFile
    {
        public string InstallLocation;
        public LibaryHandler.Libary libary;
        public bool installed;

        public LibaryFile(string InstallLocation, LibaryHandler.Libary libary) {
            this.InstallLocation = InstallLocation;
            this.libary = libary;
        }

        public LibaryFile(string InstallLocation, LibaryHandler.Libary libary, bool installed)
        {
            this.InstallLocation = InstallLocation;
            this.libary = libary;
            this.installed = installed;
        }

        public LibaryFile(bool installed) {
            this.installed = installed;
        }

        public bool IsInstalled(){
            return installed;
        }

        public override string ToString() {
            return "InstallLocation: " + InstallLocation + " | libary: " + libary + " | isInstalled: " + installed;
        }
    }
}
