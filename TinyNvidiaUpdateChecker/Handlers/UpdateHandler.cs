using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
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
                using var request = new HttpRequestMessage(HttpMethod.Head, MainConsole.updateUrl);
                using var response = MainConsole.httpClient.Send(request);
                response.EnsureSuccessStatusCode();

                var responseUri = response.RequestMessage.RequestUri.ToString();
                MainConsole.onlineVer = responseUri.Substring(responseUri.LastIndexOf("/") + 1).Substring(1);

                Console.Write("OK!");
                Console.WriteLine();
            } catch (Exception ex) {
                MainConsole.onlineVer = "0.0.0";
                Console.Write("ERROR!");
                LogManager.Log(ex.ToString(), LogManager.Level.ERROR);
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
            }

            if (new Version(MainConsole.onlineVer).CompareTo(new Version(MainConsole.offlineVer)) > 0) {
                Console.WriteLine("There is a update available for TinyNvidiaUpdateChecker!");

                if (!MainConsole.confirmDL && !MainConsole.dryRun) {
                    DialogResult dialog = MessageBox.Show("There is a client update available. Do you wish to update now? TNUC will auto-update and restart.", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialog == DialogResult.Yes) {
                        UpdateNow(args);
                    }
                }
            }

            if (MainConsole.debug) {
                Console.WriteLine($"offlineVer: {MainConsole.offlineVer}");
                Console.WriteLine($"onlineVer:  {MainConsole.onlineVer}");
            }

            Console.WriteLine();
        }

        private static void UpdateNow(string[] args)
        {
            string currentExe = Path.GetFileName(Environment.ProcessPath);

            try {
                string tempFile = Path.Combine(Path.GetTempPath(), "TinyNvidiaUpdateChecker.tmp");

                File.Move(currentExe, currentExe + ".old", true);

                Console.WriteLine();
                Console.Write("Downloading update . . . ");
                Exception ex = MainConsole.HandleDownload($"{MainConsole.updateUrl}/download/TinyNvidiaUpdateChecker.exe", tempFile);

                if (ex == null) {
                    Console.WriteLine("OK!");
                    Console.Write("Validating checksum . . . ");

                    // Validate checksum MD5
                    string md5New = CalculateMD5(tempFile);

                    if (md5New != null) {
                        string serverHash = MainConsole.ReadURL(MainConsole.checksumUrl);

                        if (md5New == serverHash) {
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
                        }
                    }
                } else {
                    Console.WriteLine("ERROR!");
                }
            } catch { }

            Console.WriteLine("Download failed");
            Console.WriteLine();
            File.Move(currentExe + ".old", currentExe, true);
        }

        public static string CalculateMD5(string filePath)
        {
            using (var md5 = MD5.Create()) {
                try {
                    using (var stream = File.OpenRead(filePath)) {
                        var hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                        return hash;
                    }
                } catch {
                    Console.WriteLine("ERROR");
                    Console.WriteLine();
                    return null;
                }
            }
        }
    }
}
