using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TinyNvidiaUpdateChecker.Handlers
{

    class LibaryHandler
    {
        private static bool is64 = Environment.Is64BitOperatingSystem;

        static List<LibaryRegistryPath> libaryRegistryList =
            [
                /* WinRAR */

                new(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\WinRAR archiver", "InstallLocation", Libary.WINRAR),
                new(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\WinRAR archiver", "InstallLocation", Libary.WINRAR),

                /* 7-Zip */

                // amd64 installer on amd64 system, or x86 on x86 system
                new(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip", "InstallLocation", Libary.SEVENZIP),

                // x86 intaller on amd64 system
                new(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip", "InstallLocation", Libary.SEVENZIP),

                // MSI amd64 installer on amd64 system, or x86 on x86 system
                new(Registry.LocalMachine, @"SOFTWARE\7-Zip", "Path", Libary.SEVENZIP),

                // MSI x86 intaller on amd64 system
                new (Registry.LocalMachine, @"SOFTWARE\WOW6432Node\7-Zip", "Path", Libary.SEVENZIP),
            ];

        static List<LibaryPath> libaryPathList =
            [
                /* 7-Zip */

                // scoop in user profile
                new([Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "scoop", "apps", "7zip", "current"], Libary.SEVENZIP),

                // scoop in Program Data
                new([Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "scoop", "apps", "7zip", "current"], Libary.SEVENZIP),

                // scoop local environment variable
                new([Environment.GetEnvironmentVariable("SCOOP", EnvironmentVariableTarget.User), "apps", "7zip", "current"], Libary.SEVENZIP),

                // scoop global environment variable
                new([Environment.GetEnvironmentVariable("SCOOP_GLOBAL", EnvironmentVariableTarget.Machine), "apps", "7zip", "current"], Libary.SEVENZIP),

                // amd64 on amd64 system, or x86 on x86 system
                new([Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip"], Libary.SEVENZIP),

                // x86 on amd64 system
                new([Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip"], Libary.SEVENZIP),
                
                // custom path defined in config file
                new([ConfigurationHandler.ReadSetting("Custom 7-ZIP Libary Path", null, false)], Libary.SEVENZIP),

                /* WinRAR */
                new([ConfigurationHandler.ReadSetting("Custom WinRAR Libary Path", null, false)], Libary.WINRAR)
            ];

        static Dictionary<string, Libary> cliList = new() {
            {"NanaZipC", Libary.NANAZIP} // NanaZip
        };

        public enum Libary
        {
            SEVENZIP,
            WINRAR,
            NANAZIP
        }

        public static LibaryFile EvaluateLibary()
        {
            foreach (var entry in cliList)
            {
                try
                {
                    using var process = new Process();

                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = entry.Key,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    process.Start();
                    string exePath = process.GetMainModuleFileName();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        string directoryPath = Path.GetDirectoryName(exePath) + @"\";
                        return new LibaryFile(directoryPath, entry.Value, true);
                    }
                }
                catch { }
            }

            foreach (var entry in libaryRegistryList)
            {
                try
                {
                    string path;

                    // since TNUC is built for x86 we need to use RegistryView.Registry64
                    if (is64)
                    {
                        using var key = RegistryKey.OpenBaseKey(entry.getRegistryHive(), RegistryView.Registry64);
                        using var localKey = key.OpenSubKey(entry.path, false);
                        path = localKey.GetValue(entry.name).ToString();
                    }
                    else
                    {
                        using var regKey = entry.key.OpenSubKey(entry.path, false);
                        path = regKey.GetValue(entry.name).ToString();
                    }

                    if (!path.EndsWith(@"\"))
                    {
                        path += @"\";
                    }

                    return new LibaryFile(path, entry.libary, true);
                }
                catch { }
            }

            foreach (var entry in libaryPathList)
            {
                try
                {
                    string path = Path.Combine(entry.path);

                    if (Path.Exists(path))
                    {
                        path += @"\";
                        return new LibaryFile(path, entry.libary, true);
                    }
                }
                catch { }
            }

            return null;
        }
    }

    internal static class Extensions
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string GetMainModuleFileName(this Process process, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ?
                fileNameBuilder.ToString() :
                null;
        }
    }

    class LibaryPath
    {
        public string[] path;
        public LibaryHandler.Libary libary;

        public LibaryPath(string[] path, LibaryHandler.Libary libary)
        {
            this.path = path;
            this.libary = libary;
        }
    }

    class LibaryRegistryPath
    {
        public RegistryKey key;
        public string path;
        public string name;
        public LibaryHandler.Libary libary;

        public LibaryRegistryPath(RegistryKey key, string path, string name, LibaryHandler.Libary libary)
        {
            this.key = key;
            this.path = path;
            this.name = name;
            this.libary = libary;
        }

        public RegistryHive getRegistryHive()
        {
            switch (key.Name)
            {
                case "HKEY_LOCAL_MACHINE":
                    return RegistryHive.LocalMachine;
                default:
                    return RegistryHive.LocalMachine;
            }

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

        public LibaryFile(string installationDirectory, LibaryHandler.Libary libary, bool isInstalled)
        {
            this.installationDirectory = installationDirectory;
            this.libary = libary;
            this.isInstalled = isInstalled;
        }

        public LibaryFile(LibaryHandler.Libary libary, bool isInstalled)
        {
            this.libary = libary;
            this.isInstalled = isInstalled;
        }

        /// <summary>
        /// Is the libary installed?
        /// </summary>
        public bool IsInstalled()
        {
            return isInstalled;
        }

        /// <summary>
        /// Libary name
        /// </summary>
        public LibaryHandler.Libary LibaryName()
        {
            return libary;
        }

        /// <summary>
        /// Get the absolute path to the libary, ending with a backslash
        /// </summary>
        public string GetInstallationDirectory()
        {
            return installationDirectory;
        }

        public override string ToString()
        {
            return $"installationDirectory: {installationDirectory} | libary: {libary} | isInstalled: {isInstalled}";
        }
    }
}
