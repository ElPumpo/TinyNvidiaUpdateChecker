using Microsoft.Win32;
using System;
using System.IO;

namespace TinyNvidiaUpdateChecker.Handlers
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
            var WinRAR = CheckWinRAR();
            var SevenZip = Check7Zip();

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
                using (var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\WinRAR archiver", false)) {
                    LogManager.Log($"WinRAR path: {regKey.GetValue("InstallLocation")}", LogManager.Level.INFO);
                    return new LibaryFile(regKey.GetValue("InstallLocation").ToString(), Libary.WINRAR, true);
                }
            } catch { }

            try {
                using (var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\WinRAR archiver", false)) {
                    LogManager.Log($"WinRAR path: {regKey.GetValue("InstallLocation")}", LogManager.Level.INFO);
                    return new LibaryFile(regKey.GetValue("InstallLocation").ToString(), Libary.WINRAR, true);
                }
            }
            catch { }

            return new LibaryFile(Libary.WINRAR, false);
        }

        private static LibaryFile Check7Zip() {
            /* Debug directory */
            if(Directory.Exists("7-Zip")) {
                LogManager.Log($"7-Zip path: {Path.GetFullPath("7-Zip") + @"\"}", LogManager.Level.INFO);
                return new LibaryFile(Path.GetFullPath("7-Zip") + @"\", Libary.SEVENZIP, true);
            }

            /* Default installer */

            // amd64 installer on amd64 system, or x86 on x86 system
            try {
                using (var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip", false)) {
                    if (regKey != null) {
                        LogManager.Log($"7-Zip path: {regKey.GetValue("InstallLocation")}", LogManager.Level.INFO);
                        return new LibaryFile(regKey.GetValue("InstallLocation").ToString(), Libary.SEVENZIP, true);
                    }
                }
            } catch { }

            // x86 intaller on amd64 system
            if (is64) {
                try {
                    using (var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip", false)) {
                        if (regKey != null) {
                            LogManager.Log($"7-Zip path: {regKey.GetValue("InstallLocation")}", LogManager.Level.INFO);
                            return new LibaryFile(regKey.GetValue("InstallLocation").ToString(), Libary.SEVENZIP, true);
                        }
                    }
                } catch { }
            }

            /* MSI installer */
            
            // amd64 installer on amd64 system, or x86 on x86 system
            try {
                using (var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\7-Zip", false)) {
                    if (regKey != null) {
                        LogManager.Log($"7-Zip path: {regKey.GetValue("Path")}", LogManager.Level.INFO);
                        return new LibaryFile(regKey.GetValue("Path").ToString(), Libary.SEVENZIP, true);
                    }
                }
            } catch { }

            // x86 intaller on amd64 system
            if (is64) {
                try {
                    using (var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\7-Zip", false)) {
                        if (regKey != null) {
                            LogManager.Log($"7-Zip path: {regKey.GetValue("Path")}", LogManager.Level.INFO);
                            return new LibaryFile(regKey.GetValue("Path").ToString(), Libary.SEVENZIP, true);
                        }
                    }
                } catch { }
            }

            /* Scoop support */
            string path;
            
            // installed in user profile
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "scoop", "apps", "7zip", "current");
            if (Directory.Exists(path)) {
                path += @"\";
                LogManager.Log($"7-Zip path: {path}", LogManager.Level.INFO);
                return new LibaryFile(path, Libary.SEVENZIP, true);
            }

            // installed in Program Data
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "scoop", "apps", "7zip", "current");
            if (Directory.Exists(path)) {
                path += @"\";
                LogManager.Log($"7-Zip path: {path}", LogManager.Level.INFO);
                return new LibaryFile(path, Libary.SEVENZIP, true);
            }

            /* Last resort checks */

            // amd64 on amd64 system, or x86 on x86 system
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip");
            if (Directory.Exists(path)) {
                path += @"\";
                LogManager.Log($"7-Zip path: {path}", LogManager.Level.INFO);
                return new LibaryFile(path, Libary.SEVENZIP, true);
            }

            // x86 on amd64 system
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip");
            if (Directory.Exists(path)) {
                path += @"\";
                LogManager.Log($"7-Zip path: {path}", LogManager.Level.INFO);
                return new LibaryFile(path, Libary.SEVENZIP, true);
            }

            return new LibaryFile(Libary.SEVENZIP, false);
        }
    }
    
    /// <summary>
    /// Libaries that can extract the nvidia driver file
    /// </summary>
    class LibaryFile
    {
        string installationDirectory;
        LibaryHandler.Libary libary;
        bool isInstalled;

        public LibaryFile(string installationDirectory, LibaryHandler.Libary libary, bool isInstalled) {
            this.installationDirectory = installationDirectory;
            this.libary = libary;
            this.isInstalled = isInstalled;
        }

        public LibaryFile(LibaryHandler.Libary libary, bool isInstalled) {
            this.libary = libary;
            this.isInstalled = isInstalled;
        }

        /// <summary>
        /// Is the libary installed?
        /// </summary>
        public bool IsInstalled() {
            return this.isInstalled;
        }

        /// <summary>
        /// Libary name
        /// </summary>
        public LibaryHandler.Libary LibaryName() {
            return libary;
        }

        /// <summary>
        /// Get the absolute path to the libary, ending with a backslash
        /// </summary>
        public string GetInstallationDirectory() {
            return installationDirectory;
        }

        public override string ToString() {
            return $"installationDirectory: {installationDirectory} | libary: {libary} | isInstalled: {isInstalled}";
        }
    }
}
