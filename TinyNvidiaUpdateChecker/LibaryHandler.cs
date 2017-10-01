using Microsoft.Win32;
using System;
using System.IO;

namespace TinyNvidiaUpdateChecker
{
    class LibaryHandler
    {

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
            if(Directory.Exists("7-Zip")) {
                LogManager.Log("7-Zip path: " + Path.GetFullPath("7-Zip") + @"\", LogManager.Level.INFO);
                return new LibaryFile(Path.GetFullPath("7-Zip") + @"\", Libary.SEVENZIP, true);
            }
            try {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip", false)) {
                    LogManager.Log("7-Zip path: " + regKey.GetValue("InstallLocation").ToString(), LogManager.Level.INFO);
                    return new LibaryFile(regKey.GetValue("InstallLocation").ToString(), Libary.SEVENZIP, true);
                }
            }
            catch (Exception) { }

            try {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip", false)) {
                    LogManager.Log("7-Zip path: " + regKey.GetValue("InstallLocation").ToString(), LogManager.Level.INFO);
                    return new LibaryFile(regKey.GetValue("InstallLocation").ToString(), Libary.SEVENZIP, true);
                }
            }
            catch (Exception) { }

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "scoop", "apps", "7zip", "current");
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
            return "InstallLocation: " + InstallLocation + " | libary: " + libary;
        }
    }
}
