﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Management;
using System.Net.NetworkInformation;
using System.ComponentModel;
using System.Xml;
using TinyNvidiaUpdateChecker.Handlers;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using HtmlAgilityPack;
using System.Net.Http;

namespace TinyNvidiaUpdateChecker
{

    public class OSClass
    {
        public string code { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class OSClassRoot : List<OSClass> { }

    class MainConsole
    {
        /// <summary>
        /// GPU metadata repo
        /// </summary>
        private readonly static string gpuMetadataRepo = "https://github.com/ZenitH-AT/nvidia-data/raw/main";

        /// <summary>
        /// URL for client update
        /// </summary>
        private readonly static string updateUrl = "https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases/latest";

        /// <summary>
        /// Current client version
        /// </summary>
        private static string offlineVer = Application.ProductVersion;

        /// <summary>
        /// Remote client version
        /// </summary>
        private static string onlineVer;

        /// <summary>
        /// Current GPU driver version
        /// </summary>
        public static string OfflineGPUVersion;

        /// <summary>
        /// Remote GPU driver version
        /// </summary>
        public static string OnlineGPUVersion;

        private static string downloadURL;
        private static string savePath;
        private static string driverFileName;
        public static string pdfURL;
        public static DateTime releaseDate;
        public static string releaseDesc;

        /// <summary>
        /// The file size of downloadURL in bytes
        /// </summary>
        public static long downloadFileSize;

        /// <summary>
        /// Show UI or go quiet mode
        /// </summary>
        public static bool showUI = true;

        /// <summary>
        /// Disable "Press any key to exit..." prompt
        /// </summary>
	public static bool noPrompt = false;

        /// <summary>
        /// Enable extended information
        /// </summary>
        public static bool debug = false;

        /// <summary>
        /// Force a prompt to download GPU drivers
        /// </summary>
        private static bool forceDL = false;

        /// <summary>
        /// Will automaticly download and install drivers
        /// </summary>
        public static bool confirmDL = false;

        /// <summary>
        /// If this value is set then it will override the default configuration file location
        /// </summary>
        public static string overrideConfigFileLocation = null;

        /// <summary>
        /// Has the intro been displayed? Because we do not want to display the intro multiple times.
        /// </summary>
        private static bool hasRunIntro = false;

        public static HttpClient httpClient = new();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [STAThread]
        private static void Main(string[] args)
        {
            string message = $"TinyNvidiaUpdateChecker v{offlineVer}";
            LogManager.Log(message, LogManager.Level.INFO);
            Console.Title = message;

            CheckArgs(args);

            RunIntro(); // will run intro if no args needs to output stuff

            if (showUI) {
                AllocConsole();

                if (!debug) {
                    GenericHandler.DisableQuickEdit();
                }
            }

            SettingManager.ConfigInit(overrideConfigFileLocation);

            CheckDependencies();

            if (SettingManager.ReadSettingBool("Check for Updates")) {
                SearchForUpdates();
            }

            Console.Write("Retrieving GPU information . . . ");

            (string gpuName, bool isNotebook) = GetGpuData();
            (int gpuId, int osId, int isDchDriver) = GetDriverMetadata(gpuName, isNotebook);
            var downloadInfo = GetDriverDownloadInfo(gpuId, osId, isDchDriver);
            var dlPrefix = SettingManager.ReadSetting("Download location");

            downloadURL = downloadInfo["DownloadURL"].ToString();
            
            // Some GPUs, such as 970M (Win10) URLs (including release notes URL) are HTTP and not HTTPS
            if (downloadURL.Contains("https://")) {
                downloadURL = downloadURL.Substring(10);
            } else {
                downloadURL = downloadURL.Substring(9);
            }

            downloadURL = $"https://{dlPrefix}{downloadURL}";

            OnlineGPUVersion = downloadInfo["Version"].ToString();
            releaseDate = DateTime.Parse(downloadInfo["ReleaseDateTime"].ToString());
            releaseDesc = Uri.UnescapeDataString(downloadInfo["ReleaseNotes"].ToString());

            // Cleanup release description
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(releaseDesc);

            // Remove image nodes
            var nodes = htmlDocument.DocumentNode.SelectNodes("//img");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (var child in nodes) child.Remove();
            }

            // Remove all links
            try {
                var hrefNodes = htmlDocument.DocumentNode.SelectNodes("//a").Where(x => x.Attributes.Contains("href"));
                foreach (var child in hrefNodes) child.Remove();
            } catch { }

            // Finally set new release description
            releaseDesc = htmlDocument.DocumentNode.OuterHtml;

            // Get real file size in bytes
            using (var request = new HttpRequestMessage(HttpMethod.Head, downloadURL)) {
                using var response = httpClient.Send(request);
                response.EnsureSuccessStatusCode();
                downloadFileSize = response.Content.Headers.ContentLength.Value;
            }

            // Get PDF release notes
            var otherNotes = Uri.UnescapeDataString(downloadInfo["OtherNotes"].ToString());

            htmlDocument.LoadHtml(otherNotes);
            IEnumerable<HtmlNode> node = htmlDocument.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href"));

            foreach (var child in node) {
                if (child.Attributes["href"].Value.Contains("release-notes.pdf")) {
                    pdfURL = child.Attributes["href"].Value.Trim();
                    break;
                }
            }

            Console.Write("OK!");
            Console.WriteLine();

            if (debug) {
                Console.WriteLine($"gpuName:     {gpuName}");
                Console.WriteLine($"gpuId:       {gpuId}");
                Console.WriteLine($"osId:        {osId}");
                Console.WriteLine($"isNotebook:  {isNotebook}");
                Console.WriteLine($"downloadURL: {downloadURL}");
                Console.WriteLine($"pdfURL:      {pdfURL}");
                Console.WriteLine($"releaseDate: {releaseDate.ToShortDateString()}");
                Console.WriteLine($"downloadFileSize:  {Math.Round((downloadFileSize / 1024f) / 1024f)} MiB");
                Console.WriteLine($"OfflineGPUVersion: {OfflineGPUVersion}");
                Console.WriteLine($"OnlineGPUVersion:  {OnlineGPUVersion}");
            }

            var updateAvailable = false;
            var iOffline = int.Parse(OfflineGPUVersion.Replace(".", string.Empty));
            var iOnline = int.Parse(OnlineGPUVersion.Replace(".", string.Empty));

            if (iOnline == iOffline) {
                Console.WriteLine("There is no new GPU driver available, you are up to date.");
            } else if (iOffline > iOnline) {
                Console.WriteLine("Your current GPU driver is newer than what NVIDIA reports!");
            } else {
                Console.WriteLine("There is a new GPU driver available to download!");
                updateAvailable = true;
            }

            if (updateAvailable || forceDL) {
                if (confirmDL) {
                    DownloadDriverQuiet(true);
                } else {
                    DownloadDriver();
                }
            }

            LogManager.Log("BYE!", LogManager.Level.INFO);
            callExit(0);
	
        }

        /// <summary>
        /// Search for client updates
        /// </summary>
        private static void SearchForUpdates()
        {
            Console.Write("Searching for Updates . . . ");

            try {
                using var request = new HttpRequestMessage(HttpMethod.Head, updateUrl);
                using var response = httpClient.Send(request);
                response.EnsureSuccessStatusCode();

                var responseUri = response.RequestMessage.RequestUri.ToString();
                onlineVer = responseUri.Substring(responseUri.LastIndexOf("/") + 1).Substring(1);

                Console.Write("OK!");
                Console.WriteLine();
            } catch (Exception ex) {
                onlineVer = "0.0.0";
                Console.Write("ERROR!");
                LogManager.Log(ex.ToString(), LogManager.Level.ERROR);
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
            }

            if (new Version(onlineVer).CompareTo(new Version(offlineVer)) > 0) {
                Console.WriteLine("There is a update available for TinyNvidiaUpdateChecker!");

                if(!confirmDL) {
                    DialogResult dialog = MessageBox.Show("There is a new client update available to download, do you want to be navigate to the official GitHub download section?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialog == DialogResult.Yes) {
                        Process.Start(new ProcessStartInfo(updateUrl[..^7]) { UseShellExecute = true });
                    }
                }
            }

            if (debug) {
                Console.WriteLine($"offlineVer: {offlineVer}");
                Console.WriteLine($"onlineVer:  {onlineVer}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Handles the command line arguments </summary>
        /// <param name="args"> Command line arguments in. Turned out that Environment.GetCommandLineArgs() wasn't any good.</param>
        private static void CheckArgs(string[] args)
        {

            /// The command line argument handler does it's work here,
            /// for a list of available arguments, use the '--help' argument.

            foreach (var arg in args)
            {

                // no window
                if (arg.ToLower() == "--quiet") {
                    FreeConsole();
                    showUI = false;
                }
				
		else if (arg.ToLower() == "--noprompt") {
                    noPrompt = true;
                }

                // erase config
                else if (arg.ToLower() == "--erase-config") {
                    if (File.Exists(SettingManager.configFile)) {
                        try {
                            File.Delete(SettingManager.configFile);
                        } catch (Exception ex) {
                            RunIntro();
                            Console.WriteLine(ex.ToString());
                            Console.WriteLine();
                        }
                    }
                }

                // enable debugging
                else if (arg.ToLower() == "--debug") {
                    debug = true;
                }

                // force driver download
                else if (arg.ToLower() == "--force-dl") {
                    forceDL = true;
                }

                // show version number
                else if (arg.ToLower() == "--version") {
                    RunIntro();
                    Console.WriteLine($"Current version is {offlineVer}");
                    Console.WriteLine();
                }

                // automaticly download driver
                else if (arg.ToLower() == "--confirm-dl") {
                    confirmDL = true;
                }

                // override the config path to working directory
                else if (arg.ToLower() == "--config-here") {
                    overrideConfigFileLocation = Path.Combine(Directory.GetCurrentDirectory(), "app.config");
                }

                // overide config file path
                else if (arg.StartsWith("--config-override=")) {
                    var locationArg = arg.Substring(18);
                    overrideConfigFileLocation = locationArg;
                }

                // help menu
                else if (arg.ToLower() == "--help") {
                    RunIntro();
                    Console.WriteLine($"Usage: {Path.GetFileName(Environment.ProcessPath)} [ARGS]");
                    Console.WriteLine();
                    Console.WriteLine("--quiet                      Runs the application quietly in the background, and will only notify the user if an update is available.");
                    Console.WriteLine("--noprompt                   Runs the application without prompting to exit.");
                    Console.WriteLine("--erase-config               Erase configuration file.");
                    Console.WriteLine("--debug                      Turn debugging on, will output more information that can be used for debugging.");
                    Console.WriteLine("--force-dl                   Force prompt to download drivers, even if the user is up-to-date - should only be used for debugging.");
                    Console.WriteLine("--version                    View version.");
                    Console.WriteLine("--confirm-dl                 Automatically download and install the driver quietly without any user interaction at all. should be used with '--quiet' for the optimal solution.");
                    Console.WriteLine("--config-here                Use the working directory as path to the configuration file.");
                    Console.WriteLine("--config-override=<path>     Override configuration file location with absolute file path.");
                    Console.WriteLine("--help                       Shows help.");
                    Environment.Exit(0);
                }

                // unknown command, right?
                else
                {
                    RunIntro();
                    Console.WriteLine($"Unknown command '{arg}', type --help for help.");
                    Console.WriteLine();
                }
            }

            // show the args if debug mode
            if (debug) {
                foreach (var arg in args) {
                    RunIntro();
                    Console.WriteLine($"Arg: {arg}");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Finds the GPU, the version and queries up to date information
        /// </summary>
        private static (string, bool) GetGpuData()
        {
            bool foundCompatibleGpu = false;
            bool isNotebook = false;
            string gpuName = "";
            var regex = new Regex(@"(?<=NVIDIA )(.*(?= \([A-Z]+\))|.*(?= [0-9]+GB)|.*(?= with Max-Q Design)|.*(?= COLLECTORS EDITION)|.*)");
            List<int> notebookChassisTypes = new() { 1, 8, 9, 10, 11, 12, 14, 18, 21, 31, 32 };

            // Check for notebook
            foreach (var obj in new ManagementClass("Win32_SystemEnclosure").GetInstances()) {
                foreach (int chassisType in obj["ChassisTypes"] as ushort[]) {
                    isNotebook = notebookChassisTypes.Contains(chassisType);
                }
            }

            // Get the local GPU metadata
            var gpus = new ManagementObjectSearcher("SELECT Name, DriverVersion FROM Win32_VideoController").Get();

            foreach (var gpu in gpus) {
                var name = gpu["Name"].ToString();

                if (Regex.IsMatch(name, @"^NVIDIA") && regex.IsMatch(name)) {
                    gpuName = regex.Match(name).Value.Trim().Replace("Super", "SUPER");
                    var rawDriverVersion = gpu["DriverVersion"].ToString().Replace(".", string.Empty);
                    OfflineGPUVersion = rawDriverVersion.Substring(rawDriverVersion.Length - 5, 5).Insert(3, ".");
                    foundCompatibleGpu = true;
                    break;
                }
            }

            if (!foundCompatibleGpu) {
                Console.Write("ERROR!");
                Console.WriteLine();
                // todo use pcilookup API https://www.pcilookup.com/api.php?action=search&vendor=10DE&device=13C2
                Console.WriteLine("No supported NVIDIA GPU was found! If you have a NVIDIA GPU then manually install a driver for it first, then use TNUC to keep it updated.");
                callExit(1);		    
            }

            return (gpuName, isNotebook);
        }

        /// <summary>
        /// Get GPU metadata
        /// </summary>
        /// <returns></returns>
        private static (int, int, int) GetDriverMetadata(string gpuName, bool isNotebook)
        {
            var gpuId = 0;
            var osId = 0;
            var isDchDriver = 0;

            // Get graphics card ID
            try {
                string gpuDataRaw;

                if (File.Exists("gpu-data.json")) {
                    gpuDataRaw = File.ReadAllText("gpu-data.json");
                    LogManager.Log("Using local gpu-data.json", LogManager.Level.INFO);
                } else {
                    gpuDataRaw = ReadURL(gpuMetadataRepo + "/gpu-data.json");
                }

                var gpuData = JObject.Parse(gpuDataRaw);
                gpuId = (int)gpuData[isNotebook ? "notebook" : "desktop"][gpuName];
            } catch (ArgumentNullException) {
                Console.Write("ERROR!");
                Console.WriteLine();
                Console.WriteLine("GPU metadata for your card does not exist! Please file an issue on GitHub and include the following information:");
                Console.WriteLine();
                Console.WriteLine($"gpuName:    {gpuName}");
                Console.WriteLine($"isNotebook: {isNotebook}");
                callExit(1);
            } catch (Exception ex) {
                Console.Write("ERROR!");
                Console.WriteLine();
                Console.WriteLine("Unable to retrieve GPU data. Do you have working internet connectivity?");
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
                callExit(1);
            }

            // Get operating system ID
            var osVersion = $"{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}";
            var osBit = Environment.Is64BitOperatingSystem ? "64" : "32";
            OSClassRoot osData = null;

            try {
                string osDataRaw;

                if (File.Exists("os-data.json")) {
                    osDataRaw = File.ReadAllText("os-data.json");
                    LogManager.Log("Using local os-data.json", LogManager.Level.INFO);
                } else {
                    osDataRaw = ReadURL(gpuMetadataRepo + "/os-data.json");
                }

                osData = JsonConvert.DeserializeObject<OSClassRoot>(osDataRaw);
            } catch (Exception ex) {
                Console.Write("ERROR!");
                Console.WriteLine();
                Console.WriteLine("Unable to retrieve OS data.");
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
                callExit(1);
            }

            foreach (var os in osData) {
                if (os.code == osVersion && Regex.IsMatch(os.name, osBit)) {
                    osId = os.id;
                    break;
                }
            }

            if (osId == 0) {
                Console.Write("ERROR!");
                Console.WriteLine();
                Console.WriteLine("Could not find a supported driver by your operating system.");
                Console.WriteLine();
                Console.WriteLine($"gpuName:   {gpuName}");
                Console.WriteLine($"osVersion: {osVersion}");
                Console.WriteLine($"osBit:     {osBit}");
                callExit(1);                    
            }

            // Check for DCH for newer drivers
            using (var regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\nvlddmkm", false)) {
                if (regKey != null && regKey.GetValue("DCHUVen") != null) {
                    isDchDriver = 1;
                }
            }

            return (gpuId, osId, isDchDriver);
        }

        private static JObject GetDriverDownloadInfo(int gpuId, int osId, int isDchDriver) {
            try {
                var ajaxDriverLink = "https://gfwsl.geforce.com/services_toolkit/services/com/nvidia/services/AjaxDriverService.php?func=DriverManualLookup";
                ajaxDriverLink += $"&pfid={gpuId}&osID={osId}&dch={isDchDriver}";

                JObject driverObj = JObject.Parse(ReadURL(ajaxDriverLink));

                // Check if driver was found
                if ((int)driverObj["Success"] == 1) {

                    // If the operating system has support for DCH drivers, and DCH drivers are currently not installed, then serach for DCH drivers too.
                    // Non-DCH drivers are discontinued. Not searching for DCH drivers will result in users having outdated graphics drivers, and we don't want that.
                    if (Environment.Version.Build > 10240 && isDchDriver == 0) {
                        ajaxDriverLink = ajaxDriverLink.Substring(0, ajaxDriverLink.Length - 1) + "1";
                        JObject driverObjDCH = JObject.Parse(ReadURL(ajaxDriverLink));

                        if ((int)driverObjDCH["Success"] == 1) {
                            return (JObject)driverObjDCH["IDS"][0]["downloadInfo"];
                        }
                    }

                    return (JObject)driverObj["IDS"][0]["downloadInfo"];
                } else {
                    throw new ArgumentOutOfRangeException();
                }
            } catch (ArgumentOutOfRangeException) {
                Console.Write("ERROR!");
                Console.WriteLine();
                Console.WriteLine("No NVIDIA driver was found for your system configuration.");
                Console.WriteLine();
                Console.WriteLine($"gpuId:       {gpuId}");
                Console.WriteLine($"osId:        {osId}");
                Console.WriteLine($"isDchDriver: {isDchDriver}");
            } catch (Exception ex) {
                Console.Write("ERROR!");
                Console.WriteLine();
                Console.WriteLine("Unable to interact with NVIDIA API.");
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
            }
	
            callExit(1);

            return null;
        }

        private static string ReadURL(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = httpClient.Send(request);
            response.EnsureSuccessStatusCode();

            using var stream = response.Content.ReadAsStream();
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Check if dependencies are all OK
        /// </summary>
        private static void CheckDependencies()
        {

            // Check internet connection
            Console.Write("Verifying internet connection . . . ");
            if (NetworkInterface.GetIsNetworkAvailable()) {
                Console.Write("OK!");
                Console.WriteLine();
            } else {
                Console.Write("ERROR!");
                Console.WriteLine();
                Console.WriteLine("You are not connected to the internet!");
                callExit(2);
            }

            if (SettingManager.ReadSettingBool("Minimal install")) {
                if (LibaryHandler.EvaluateLibary() == null) {
                    Console.WriteLine("Doesn't seem like either WinRAR or 7-Zip is installed! We are disabling the minimal install feature for you.");
                    SettingManager.SetSetting("Minimal install", "false");
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Downloads the driver and some other stuff
        /// </summary>
        private static void DownloadDriver()
        {
            DriverDialog.ShowGUI();

            if (DriverDialog.selectedBtn == DriverDialog.SelectedBtn.DLEXTRACT) {
                // download and save (and extract)
                Console.WriteLine();
                bool error = false;
                driverFileName = downloadURL.Split('/').Last(); // retrives file name from url

                try {
                   string title = "Where do you want to save the driver?";

                    if (SettingManager.ReadSettingBool("Minimal install")) {
                        title += " (you should select a empty folder)";
                    }

                    using var dialog = new FolderBrowserDialog {
                        Description = title,
                        UseDescriptionForTitle = true,
                        ShowNewFolderButton = true
                    };

                    if (dialog.ShowDialog() == DialogResult.OK) {
                        savePath = dialog.SelectedPath + @"\";
                    } else {
                        Console.WriteLine("User closed dialog!");
                        return;
                    }

                    if (File.Exists(savePath + driverFileName) && !DoesDriverFileSizeMatch(savePath + driverFileName)) {
                        LogManager.Log($"Deleting {savePath}{driverFileName} because its length doesn't match!", LogManager.Level.INFO);
                        File.Delete(savePath + driverFileName);
                    }

                    // don't download driver if it already exists
                    Console.Write("Downloading the driver . . . ");
                    if (showUI && !File.Exists(savePath + driverFileName)) {
                        var ex = HandleDownload(downloadURL, savePath + driverFileName);

                        if (ex != null) {
                            throw ex;
                        }
                    }
                    // show the progress bar gui
                    else if(!showUI && !File.Exists(savePath + driverFileName)) {
                        using DownloaderForm dlForm = new();
                        dlForm.Show();
                        dlForm.Focus();
                        dlForm.DownloadFile(new Uri(downloadURL), savePath + driverFileName);
                        dlForm.Close();
                    } else {
                        LogManager.Log("Driver is already downloaded", LogManager.Level.INFO);
                    }

                } catch (Exception ex) {
                    error = true;
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine();
                }

                if (!error) {
                    Console.Write("OK!");
                    Console.WriteLine();
                }

                if (debug) {
                    Console.WriteLine($"savePath: {savePath}");
                }

                if (SettingManager.ReadSettingBool("Minimal install")) {
                    MakeInstaller(false);
                }
            } else if (DriverDialog.selectedBtn == DriverDialog.SelectedBtn.DLINSTALL) {
                DownloadDriverQuiet(false);
            }
        }

        /// <summary>
        /// Downloads and installs the driver without user interaction
        /// </summary>
        private static void DownloadDriverQuiet(bool minimized)
        {
            driverFileName = downloadURL.Split('/').Last(); // retrives file name from url
            savePath = Path.GetTempPath();

            var FULL_PATH_DIRECTORY = savePath + OnlineGPUVersion + @"\";
            var FULL_PATH_DRIVER = FULL_PATH_DIRECTORY + driverFileName;

            savePath = FULL_PATH_DIRECTORY;

            Directory.CreateDirectory(FULL_PATH_DIRECTORY);

            if (File.Exists(FULL_PATH_DRIVER) && !DoesDriverFileSizeMatch(FULL_PATH_DRIVER)) {
                LogManager.Log($"Deleting {FULL_PATH_DRIVER} because its length doesn't match!", LogManager.Level.INFO);
                File.Delete(savePath + driverFileName);
            }

            if (!File.Exists(FULL_PATH_DRIVER)) {
                Console.Write("Downloading the driver . . . ");

                if (showUI || confirmDL) {
                    try {
                        var ex = HandleDownload(downloadURL, FULL_PATH_DRIVER);

                        if (ex != null) {
                            throw ex;
                        }

                        Console.Write("OK!");
                        Console.WriteLine();
                    } catch (Exception ex) {
                        Console.Write("ERROR!");
                        Console.WriteLine();
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine();
                    }
                } else {
                    using DownloaderForm dlForm = new();
                    dlForm.Show();
                    dlForm.Focus();
                    dlForm.DownloadFile(new Uri(downloadURL), FULL_PATH_DRIVER);
                    dlForm.Close();
                }
            }

            if (SettingManager.ReadSettingBool("Minimal install")) {
                MakeInstaller(minimized);
            }

            try {
                Console.WriteLine();
                Console.Write("Executing driver installer . . . ");

                var minimalInstaller = SettingManager.ReadSettingBool("Minimal install");
                var arguments = minimized ? "/s /noreboot" : "/nosplash";
                var fileName = minimalInstaller ? FULL_PATH_DIRECTORY + "setup.exe" : FULL_PATH_DRIVER;

                ProcessStartInfo startInfo = new(fileName);
                startInfo.UseShellExecute = true;
                startInfo.Arguments = arguments;

                Process.Start(startInfo).WaitForExit();
                Console.Write("OK!");
            } catch (Exception ex) {
                Console.WriteLine("An error occurred preventing the driver installer to execute!");
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine();

            try {
                Directory.Delete(FULL_PATH_DIRECTORY, true);
                Console.WriteLine($"Cleaned up: {FULL_PATH_DIRECTORY}");
            } catch {
                Console.WriteLine($"Could not cleanup: {FULL_PATH_DIRECTORY}");
            }

        }

        /// <summary>
        /// Shared method for the accual downloading of a file with the command line progress bar.
        /// </summary>
        /// <param name="url">URL path for download</param>
        /// <param name="path">Absolute file path</param>
        /// <returns></returns>
        private static Exception HandleDownload(string url, string path)
        {
            Exception ex = null;
            path += ".part"; // add 'partial' to file name making it easier to identify as an incomplete download

            // if a partial file download exists, delete it now
            if (File.Exists(path)) {
                File.Delete(path);
            }

            try {
                using WebClient webClient = new();
                var notifier = new AutoResetEvent(false);
                var progress = new Handlers.ProgressBar();

                webClient.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e) {
                    progress.Report((double)e.ProgressPercentage / 100);
                };

                webClient.DownloadFileCompleted += delegate (object sender, AsyncCompletedEventArgs e) {
                    if (e.Cancelled || e.Error != null) {
                        File.Delete(path);
                        ex = e.Error;
                    }

                    File.Move(path, path.Substring(0, path.Length - 5)); // rename back
                    notifier.Set();
                };

                webClient.DownloadFileAsync(new Uri(url), path);
                notifier.WaitOne();
                progress.Dispose();

                return ex;
            } catch (Exception ex2) {
                return ex2;
            }
        }

        /// <summary>
        /// Remove telementry and only extract basic drivers
        /// </summary>
        private static void MakeInstaller(bool silent)
        {
            Console.WriteLine();
            Console.Write("Extracting drivers . . . ");

            var error = false;
            var libaryFile = LibaryHandler.EvaluateLibary();
            string[] filesToExtract = { "Display.Driver", "NVI2", "EULA.txt", "license.txt", "ListDevices.txt", "setup.cfg", "setup.exe" };

            try {
                File.WriteAllLines(savePath + "inclList.txt", filesToExtract);
            } catch (Exception ex) {
                error = true;
                Console.Write("ERROR!");
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
            }

            string fullDriverPath = @"""" + savePath + driverFileName + @"""";

            if (libaryFile.LibaryName() == LibaryHandler.Libary.WINRAR) {
                using var WinRAR = new Process();
                WinRAR.StartInfo.FileName = libaryFile.GetInstallationDirectory() + "winrar.exe";
                WinRAR.StartInfo.WorkingDirectory = savePath;
                WinRAR.StartInfo.Arguments = $@"X {fullDriverPath} -N@""inclList.txt""";
                if (silent) WinRAR.StartInfo.Arguments += " -ibck -y";
                WinRAR.StartInfo.UseShellExecute = false;

                try {
                    WinRAR.Start();
                    WinRAR.WaitForExit();
                } catch (Exception ex) {
                    error = true;
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                }
            } else if (libaryFile.LibaryName() == LibaryHandler.Libary.SEVENZIP) {
                using var SevenZip = new Process();
                if (silent) {
                    SevenZip.StartInfo.FileName = libaryFile.GetInstallationDirectory() + "7z.exe";
                } else {
                    SevenZip.StartInfo.FileName = libaryFile.GetInstallationDirectory() + "7zG.exe";
                }

                SevenZip.StartInfo.WorkingDirectory = savePath;
                SevenZip.StartInfo.Arguments = $"x {fullDriverPath} @inclList.txt";
                if (silent) SevenZip.StartInfo.Arguments += " -y";
                SevenZip.StartInfo.UseShellExecute = false;
                SevenZip.StartInfo.CreateNoWindow = true; // don't show the console in our console!

                try {
                    Thread.Sleep(1000);
                    SevenZip.Start();
                    SevenZip.WaitForExit();
                } catch (Exception ex) {
                    error = true;
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                }
            } else {
                Console.WriteLine("Could not identify a possible extractor! We should panic.");
                error = true;
            }

            // remove new EULA files from the installer config, or else the installer throws error codes
            // author https://github.com/cywq
            if (!error) {
                var xmlDocument = new XmlDocument();
                string setupFile = savePath + "setup.cfg";
                string[] linesToDelete = { "${{EulaHtmlFile}}", "${{FunctionalConsentFile}}", "${{PrivacyPolicyFile}}" };

                xmlDocument.Load(setupFile);

                foreach (var line in linesToDelete) {
                    var node = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode($"/setup/manifest/file[@name=\"{line}\"]");

                    if (node != null) {
                        node.ParentNode.RemoveChild(node);
                    }
                }

                xmlDocument.Save(setupFile);
            }

            if (!error) {
                Console.Write("OK!");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Intro with legal message, moved to reduce lines that ultimately does the same thing.
        /// </summary>
        private static void RunIntro()
        {
            if (!hasRunIntro) {
                hasRunIntro = true;
                //Console.WriteLine($"TinyNvidiaUpdateChecker v{offlineVer} dev build");
                Console.WriteLine($"TinyNvidiaUpdateChecker v{offlineVer}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Check for passed argument and prompt for exit if applicable
        /// </summary>
        /// 
        private static void callExit(int exitNum)
        {
            if (!noPrompt)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
            }
		
            if (showUI & !noPrompt) Console.ReadKey();
            Environment.Exit(exitNum);
        }	    
	    
        private static bool DoesDriverFileSizeMatch(string absoluteFilePath) {
            return new FileInfo(absoluteFilePath).Length == downloadFileSize;
        }
    }
}
