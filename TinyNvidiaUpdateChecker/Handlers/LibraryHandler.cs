using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static TinyNvidiaUpdateChecker.Handlers.LibraryHandler;

namespace TinyNvidiaUpdateChecker.Handlers
{

    class LibraryHandler
    {
        private static bool is64 = Environment.Is64BitOperatingSystem;

        static List<LibraryRegistryPath> libraryRegistryList 
        =
            [
                /* WinRAR */

                new(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\WinRAR archiver", "InstallLocation", Library.WINRAR),
                new(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\WinRAR archiver", "InstallLocation", Library.WINRAR),

                /* 7-Zip */

                // amd64 installer on amd64 system, or x86 on x86 system
                new(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip", "InstallLocation", Library.SEVENZIP),

                // x86 intaller on amd64 system
                new(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\7-Zip", "InstallLocation", Library.SEVENZIP),

                // MSI amd64 installer on amd64 system, or x86 on x86 system
                new(Registry.LocalMachine, @"SOFTWARE\7-Zip", "Path", Library.SEVENZIP),

                // MSI x86 intaller on amd64 system
                new (Registry.LocalMachine, @"SOFTWARE\WOW6432Node\7-Zip", "Path", Library.SEVENZIP),
            ];

        static List<LibraryPath> LibraryPathList =
            [
                /* 7-Zip */

                // scoop in user profile
                new([Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "scoop", "apps", "7zip", "current"], Library.SEVENZIP),

                // scoop in Program Data
                new([Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "scoop", "apps", "7zip", "current"], Library.SEVENZIP),

                // scoop local environment variable
                new([Environment.GetEnvironmentVariable("SCOOP", EnvironmentVariableTarget.User), "apps", "7zip", "current"], Library.SEVENZIP),

                // scoop global environment variable
                new([Environment.GetEnvironmentVariable("SCOOP_GLOBAL", EnvironmentVariableTarget.Machine), "apps", "7zip", "current"], Library.SEVENZIP),

                // amd64 on amd64 system, or x86 on x86 system
                new([Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip"], Library.SEVENZIP),

                // x86 on amd64 system
                new([Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip"], Library.SEVENZIP),
                
                // custom path defined in config file
                new([ConfigurationHandler.ReadSetting("Custom 7-ZIP Library Path", null, false)], Library.SEVENZIP),

                /* WinRAR */
                new([ConfigurationHandler.ReadSetting("Custom WinRAR Library Path", null, false)], Library.WINRAR)
            ];

        static Dictionary<string, Library> cliList = new() {
            {"NanaZipC", Library.NANAZIP} // NanaZip
        };

        public enum Library
        {
            SEVENZIP,
            WINRAR,
            NANAZIP
        }

        public static LibraryFile EvaluateLibrary()
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
                        return new LibraryFile(directoryPath, entry.Value, true);
                    }
                }
                catch { }
            }

            foreach (var entry in libraryRegistryList)
            
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

                    string exe = LibraryPath.GetExeFromLibrary(entry.library);
                    string exePath = Path.Combine(path, exe);

                    if (Path.Exists(exePath))
                    {
                        return new LibraryFile(path, entry.library, true);
                    }
                }
                catch { }
            }

            foreach (var entry in LibraryPathList)
            {
                if (entry.validate())
                {
                    string path = Path.Combine(entry.path) + @"\";
                    return new LibraryFile(path, entry.library, true);
                }
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

    class LibraryPath
    {
        public string[] path;
        public LibraryHandler.Library library;

        public LibraryPath(string[] path, LibraryHandler.Library library)
        {
            this.path = path;
            this.library = library;
        }

        public bool validate()
        {
            try
            {
                string exe = GetExeFromLibrary(this.library);
                string path = Path.Combine([.. this.path, exe]);

                if (Path.Exists(path))
                {
                    return true;
                }
            }
            catch { }

            return false;
        }

        public static string GetExeFromLibrary(Library library)
        {
            return library switch
            {
                Library.SEVENZIP => "7z.exe",
                Library.WINRAR => "winrar.exe",
                _ => "",
            };
        }
    }

    class LibraryRegistryPath
    {
        public RegistryKey key;
        public string path;
        public string name;
        public LibraryHandler.Library library;

        public LibraryRegistryPath(RegistryKey key, string path, string name, LibraryHandler.Library library)
        {
            this.key = key;
            this.path = path;
            this.name = name;
            this.library = library;
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
    class LibraryFile
    {
        string installationDirectory;
        LibraryHandler.Library library;
        bool isInstalled;

        public LibraryFile(string installationDirectory, LibraryHandler.Library library, bool isInstalled)
        {
            this.installationDirectory = installationDirectory;
            this.library = library;
            this.isInstalled = isInstalled;
        }

        public LibraryFile(LibraryHandler.Library library, bool isInstalled)
        {
            this.library = library;
            this.isInstalled = isInstalled;
        }

        /// <summary>
        /// Is the library installed?
        /// </summary>
        public bool IsInstalled()
        {
            return isInstalled;
        }

        /// <summary>
        /// Library name
        /// </summary>
        public LibraryHandler.Library LibraryName()
        {
            return library;
        }

        /// <summary>
        /// Get the absolute path to the library, ending with a backslash
        /// </summary>
        public string GetInstallationDirectory()
        {
            return installationDirectory;
        }

        public override string ToString()
        {
            return $"installationDirectory: {installationDirectory} | library: {library} | isInstalled: {isInstalled}";
        }
    }
}
