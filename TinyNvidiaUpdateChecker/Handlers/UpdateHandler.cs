using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker.Handlers
{
    internal class UpdateHandler
    {
        public static void SearchForUpdate(string[] args)
        {
            Console.Write("Searching for Update . . . ");

            try {
                string response = MainConsole.ReadURL(MainConsole.updateUrl);
                GitHubAPIReleaseRoot release = JsonConvert.DeserializeObject<GitHubAPIReleaseRoot>(response);
                MainConsole.onlineVer = release.tag_name[1..];

                Asset exeFile = release.assets.Where(x => x.name == "TinyNvidiaUpdateChecker.exe").First();
                string downloadUrl = exeFile.browser_download_url;
                string serverHash = exeFile.digest[7..];
                string changelog = release.body;

                Console.Write("OK!");
                Console.WriteLine();

                if (new Version(MainConsole.onlineVer).CompareTo(new Version(MainConsole.offlineVer)) > 0) {
                    Console.WriteLine("There is a update available for TinyNvidiaUpdateChecker!");

                    if (!MainConsole.confirmDL && !MainConsole.dryRun) {
                        TaskDialogButton[] buttons = [
                            new("Update Now") { Tag = "update" },
                            new("Ignore") { Tag = "no" }
                        ];

                        string dialog = ConfigurationHandler.ShowButtonDialog("New Client Update Available", changelog, TaskDialogIcon.Information, buttons);

                        if (dialog == "update") {
                            UpdateNow(args, downloadUrl, serverHash);
                        }
                    }
                }
            } catch (Exception ex) {
                MainConsole.onlineVer = "0.0.0";
                Console.Write("ERROR!");
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
            }

            if (MainConsole.debug) {
                Console.WriteLine($"offlineVer: {MainConsole.offlineVer}");
                Console.WriteLine($"onlineVer:  {MainConsole.onlineVer}");
            }

            Console.WriteLine();
        }

        private static void UpdateNow(string[] args, string downloadUrl, string serverHash)
        {
            string currentExe = Path.GetFullPath(Environment.ProcessPath);

            try {
                string tempFile = Path.Combine(Path.GetTempPath(), "TinyNvidiaUpdateChecker.tmp");
                File.Move(currentExe, currentExe + ".old", true);

                Console.WriteLine();
                Console.Write("Downloading update . . . ");

                MainConsole.HandleDownload(downloadUrl, tempFile).GetAwaiter().GetResult();

                Console.WriteLine("OK!");
                Console.Write("Validating checksum . . . ");

                // Validate checksum SHA256
                string tempHash = CalculateSHA256(tempFile);

                if (tempHash != null && tempHash == serverHash) {
                    Console.WriteLine("OK!");
                    Console.WriteLine();

                    File.Move(tempFile, currentExe);
                    Console.WriteLine("Relaunching now!");

                    string runArgs = string.Join(" ", args) + " --cleanup-update";
                    Process.Start(new ProcessStartInfo(currentExe) { UseShellExecute = true, Arguments = runArgs });
                    Environment.Exit(0);
                } else {
                    Console.WriteLine("ERROR!");
                    Console.WriteLine("Checksum mismatch!");
                    Console.WriteLine();
                    Console.WriteLine($"Calculated Hash: {tempHash}");
                    Console.WriteLine($"Server Hash:     {serverHash}");
                }
            } catch { }

            Console.WriteLine("Update failed, please update manually.");
            Console.WriteLine();
            File.Move(currentExe + ".old", currentExe, true);
        }

        public static string CalculateSHA256(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                try
                {
                    using (FileStream stream = File.OpenRead(filePath))
                    {
                        string hash = BitConverter.ToString(sha256.ComputeHash(stream))
                            .Replace("-", "")
                            .ToLowerInvariant();
                        return hash;
                    }
                }
                catch
                {
                    Console.WriteLine("ERROR");
                    Console.WriteLine();
                    return null;
                }
            }
        }
    }
}
