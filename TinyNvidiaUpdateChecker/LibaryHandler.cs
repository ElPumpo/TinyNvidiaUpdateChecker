using Microsoft.Win32;
using System;

namespace TinyNvidiaUpdateChecker
{
    class LibaryHandler
    {

        public enum Libary
        {
            SEVENZIP,
            WINRAR,
            ERROR
        }

        public static LibaryFile EvaluateLibary() {
            LibaryFile WinRAR = CheckWinRAR();
            LibaryFile SevenZip = Check7Zip();

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
            } catch (Exception) {
                return new LibaryFile(false);
            }
        }

        private static LibaryFile Check7Zip() {
            try {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip", false)) {
                    LogManager.Log("7-Zip path: " + regKey.GetValue("InstallLocation").ToString(), LogManager.Level.INFO);
                    return new LibaryFile(regKey.GetValue("InstallLocation").ToString(), Libary.SEVENZIP, true);
                }
            }
            catch (Exception) {
                return new LibaryFile(false);
            }
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
