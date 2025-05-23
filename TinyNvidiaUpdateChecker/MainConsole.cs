using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Xml;
using TinyNvidiaUpdateChecker.Handlers;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using HtmlAgilityPack;
using System.Net.Http;
using HttpClientProgress;
using System.Threading.Tasks;

namespace TinyNvidiaUpdateChecker
{

    class MainConsole
    {
        /// <summary>
        /// GPU metadata repo
        /// </summary>
        public readonly static string gpuMetadataRepo = "https://github.com/ZenitH-AT/nvidia-data/raw/main";

        /// <summary>
        /// URL for client update
        /// </summary>
        public readonly static string updateUrl = "https://api.github.com/repos/ElPumpo/TinyNvidiaUpdateChecker/releases/latest";

        /// <summary>
        /// Current client version
        /// </summary>
        public static string offlineVer = Application.ProductVersion;

        /// <summary>
        /// Remote client version
        /// </summary>
        public static string onlineVer;

        /// <summary>
        /// Current GPU driver version
        /// </summary>
        public static string OfflineGPUVersion;

        /// <summary>
        /// Remote GPU driver version
        /// </summary>
        public static string OnlineGPUVersion;

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
        /// Dry run
        /// </summary>
        public static bool dryRun = false;    

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
        /// Override chassis type. Used for example notebook systems with desktop e-GPUs
        /// </summary>
        public static int overrideChassisType = 0;

        /// <summary>
        /// Has the intro been displayed? Because we do not want to display the intro multiple times.
        /// </summary>
        private static bool hasRunIntro = false;

        public static HttpClient httpClient = new();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;

        static bool debuggerAttached = Debugger.IsAttached;

        static bool consoleAttached = false;

        [STAThread]
        private static void Main(string[] args)
        {
            string message = $"TinyNvidiaUpdateChecker v{offlineVer}";
            
            CheckArgs(args);

            if (showUI && !debuggerAttached) {
                if (GetConsoleWindow() == IntPtr.Zero) {
                    bool success = AttachConsole(ATTACH_PARENT_PROCESS);
                    consoleAttached = true;

                    if (success) {
                        WriteLine();
                        noPrompt = true; // no prompt needed, we are in existing console
                    } else {
                        AllocConsole();
                    }
                }

                Console.Title = message;

                if (!debug) {
                    GenericHandler.DisableQuickEdit();
                }
            } else if (!showUI && !debuggerAttached) {
                FreeConsole();
            }

            RunIntro();

            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36");

            ConfigurationHandler.ConfigInit(overrideConfigFileLocation);

            CheckDependencies();

            if (ConfigurationHandler.ReadSettingBool("Check for Updates")) {
                UpdateHandler.SearchForUpdate(args);
            }

            Write("Retrieving GPU information . . . ");

            MetadataHandler.PrepareCache();
            (GPU gpu, int osId) = GetDriverMetadata();
            JObject downloadInfo = GetDriverDownloadInfo(gpu.id, osId, gpu.isDch);
            string dlPrefix = ConfigurationHandler.ReadSetting("Download location");

            OfflineGPUVersion = gpu.version;
            string downloadURL = downloadInfo["DownloadURL"].ToString();
            
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

            Write("OK!");
            WriteLine();

            if (debug) {
                WriteLine($"gpuId:       {gpu.id}");
                WriteLine($"osId:        {osId}");
                WriteLine($"isDchDriver: {gpu.isDch}");
                WriteLine($"downloadURL: {downloadURL}");
                WriteLine($"pdfURL:      {pdfURL}");
                WriteLine($"releaseDate: {releaseDate.ToShortDateString()}");
                WriteLine($"downloadFileSize:  {Math.Round((downloadFileSize / 1024f) / 1024f)} MiB");
                WriteLine($"OfflineGPUVersion: {OfflineGPUVersion}");
                WriteLine($"OnlineGPUVersion:  {OnlineGPUVersion}");
            }

            var updateAvailable = false;
            var iOffline = int.Parse(OfflineGPUVersion.Replace(".", string.Empty));
            var iOnline = int.Parse(OnlineGPUVersion.Replace(".", string.Empty));

            if (iOnline == iOffline) {
                WriteLine("There is no new GPU driver available, you are up to date.");
            } else if (iOffline > iOnline) {
                WriteLine("Your current GPU driver is newer than what NVIDIA reports!");
            } else {
                WriteLine("There is a new GPU driver available to download!");
                updateAvailable = true;
            }

            if ((updateAvailable || forceDL) && !dryRun) {
                if (confirmDL) {
                    DownloadDriverQuiet(true, downloadURL);
                } else {
                    DownloadDriver(downloadURL);
                }
            }

            callExit(0);
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
                    showUI = false;
                }

                else if (arg.ToLower() == "--noprompt") {
                    noPrompt = true;
                }

                else if (arg.ToLower() == "--dry-run") {
                    dryRun = true;
                }

                // erase config
                else if (arg.ToLower() == "--erase-config") {
                    if (File.Exists(ConfigurationHandler.configFilePath)) {
                        try {
                            File.Delete(ConfigurationHandler.configFilePath);
                        } catch (Exception ex) {
                            RunIntro();
                            WriteLine(ex.ToString());
                            WriteLine();
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
                    WriteLine($"Current version is {offlineVer}");
                    WriteLine();
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

                // delete old file
                else if (arg.ToLower() == "--cleanup-update") {
                    File.Delete(Path.GetFullPath(Environment.ProcessPath) + ".old");
                }

                // help menu
                else if (arg.ToLower() == "--help") {
                    RunIntro();
                    WriteLine($"Usage: {Path.GetFileName(Environment.ProcessPath)} [ARGS]");
                    WriteLine();
                    WriteLine("--quiet                      Runs the application quietly in the background, and will only notify the user if an update is available.");
                    WriteLine("--noprompt                   Runs the application without prompting to exit.");
                    WriteLine("--dry-run                    Perform a dry run.");
                    WriteLine("--erase-config               Erase configuration file.");
                    WriteLine("--debug                      Turn debugging on, will output more information that can be used for debugging.");
                    WriteLine("--force-dl                   Force prompt to download drivers, even if the user is up-to-date - should only be used for debugging.");
                    WriteLine("--version                    View version.");
                    WriteLine("--confirm-dl                 Automatically download and install the driver quietly without any user interaction at all. should be used with '--quiet' for the optimal solution.");
                    WriteLine("--config-here                Use the working directory as path to the configuration file.");
                    WriteLine("--config-override=<path>     Override configuration file location with absolute file path.");
                    WriteLine("--help                       Shows help.");
                    WriteLine("--override-desktop           Override automatic desktop/notebook identification.");
                    WriteLine("--override-notebook          Override automatic desktop/notebook identification.");
                    Environment.Exit(0);
                }

                else if (arg.ToLower() == "--override-desktop") {
                    overrideChassisType = 3;
                }

                else if (arg.ToLower() == "--override-notebook") {
                    overrideChassisType = 9;
                }

                // unknown command, right?
                else
                {
                    RunIntro();
                    WriteLine($"Unknown command '{arg}', type --help for help.");
                    WriteLine();
                }
            }

            // show the args if debug mode
            if (debug) {
                foreach (var arg in args) {
                    RunIntro();
                    WriteLine($"Arg: {arg}");
                }
                WriteLine();
            }
        }

        /// <summary>
        /// Finds the GPU, the version and queries up to date information
        /// </summary>

        private static (GPU, int) GetDriverMetadata(bool forceRecache = false)
        {
            bool isNotebook = false;
            bool isDchDriver = false; // TODO rewrite for each GPU
            Regex nameRegex = new(@"(?<=NVIDIA )(.*(?= \([A-Z]+\))|.*(?= [0-9]+GB)|.*(?= with Max-Q Design)|.*(?= COLLECTORS EDITION)|.*)");
            List<int> notebookChassisTypes = [1, 8, 9, 10, 11, 12, 14, 18, 21, 31, 32];
            List<GPU> gpuList = [];

            // Check for notebook
            // TODO rewrite and identify GPUs properly
            if (overrideChassisType == 0) {
                foreach (var obj in new ManagementClass("Win32_SystemEnclosure").GetInstances()) {
                    foreach (int chassisType in obj["ChassisTypes"] as ushort[]) {
                        isNotebook = notebookChassisTypes.Contains(chassisType);
                    }
                }
            } else {
                isNotebook = notebookChassisTypes.Contains(overrideChassisType);
            }

            // Get operating system ID
            OSClassRoot osData = MetadataHandler.RetrieveOSData();
            string osVersion = $"{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}";
            string osBit = Environment.Is64BitOperatingSystem ? "64" : "32";
            int osId = 0;

            if (osVersion == "10.0" && Environment.OSVersion.Version.Build >= 22000) {
                foreach (OSClass os in osData) {
                    if (Regex.IsMatch(os.name, "Windows 11")) {
                        osId = os.id;
                        break;
                    }
                }
            } else {
                foreach (OSClass os in osData) {
                    if (os.code == osVersion && Regex.IsMatch(os.name, osBit)) {
                        osId = os.id;
                        break;
                    }
                }
            }

            if (osId == 0) {
                Write("ERROR!");
                WriteLine();
                WriteLine("No NVIDIA driver was found for this operating system configuration. Make sure TNUC is updated.");
                WriteLine();
                WriteLine($"osVersion: {osVersion}");
                callExit(1);
            }

            // Check for DCH for newer drivers
            // TODO do we know if this applies to every GPU?
            using (var regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\nvlddmkm", false)) {
                if (regKey != null && regKey.GetValue("DCHUVen") != null) {
                    isDchDriver = true;
                }
            }

            // Scan computer for GPUs
            foreach (ManagementBaseObject gpu in new ManagementObjectSearcher("SELECT Name, DriverVersion, PNPDeviceID FROM Win32_VideoController").Get()) {
                string rawName = gpu["Name"].ToString();
                string rawVersion = gpu["DriverVersion"].ToString().Replace(".", string.Empty);
                string pnp = gpu["PNPDeviceID"].ToString();

                // Is it a GPU?
                if (pnp.Contains("&DEV_")) {
                    string[] split = pnp.Split("&DEV_");
                    string vendorID = split[0][^4..];
                    string deviceID = split[1][..4];

                    // Are drivers installed for this GPU? If not Windows reports a generic GPU name which is not sufficient
                    if (Regex.IsMatch(rawName, @"^NVIDIA") && nameRegex.IsMatch(rawName)) {
                        string gpuName = nameRegex.Match(rawName).Value.Trim().Replace("Super", "SUPER");
                        string cleanVersion = rawVersion.Substring(rawVersion.Length - 5, 5).Insert(3, ".");

                        gpuList.Add(new GPU(gpuName, cleanVersion, vendorID, deviceID, true, isNotebook, isDchDriver));

                    // Name does not match but the vendor is NVIDIA, use API to lookup its name
                    } else if (vendorID == "10de") {
                        gpuList.Add(new GPU(rawName, rawVersion, vendorID, deviceID, false, isNotebook, isDchDriver));
                    }
                }
            }

            // If no drivers were found then query PCI Lookup API for each GPU
            // TODO: PCI Lookup API requires seperate GPU name sanitation code which has not been developed yet
            // See issue #215
            Regex apiRegex = new(@"([A-Za-z0-9]+( [A-Za-z0-9]+)+)");

            foreach (GPU gpu in gpuList.Where(x => !x.isValidated)) {
                string url = $"https://www.pcilookup.com/api.php?action=search&vendor={gpu.vendorId}&device={gpu.deviceId}";
                string rawData = ReadURL(url);
                PCILookupClassRoot apiResponse = JsonConvert.DeserializeObject<PCILookupClassRoot>(rawData);

                if (apiResponse != null && apiResponse.Count > 0) {
                    string rawName = apiResponse[0].desc;
                    string rawVersion = gpu.version;

                    if (apiRegex.IsMatch(rawName)) {
                        gpu.name = apiRegex.Match(rawName).Value.Trim();
                        gpu.version = rawVersion.Substring(rawVersion.Length - 5, 5).Insert(3, ".");
                        gpu.isValidated = true;
                    }
                }
            }

            foreach (GPU gpu in gpuList.Where(x => x.isValidated)) {
                (bool success, int gpuId) = MetadataHandler.GetGpuIdFromName(gpu.name, gpu.isNotebook);

                if (success) {
                    gpu.id = gpuId;
                } else {
                    // check the other type, perhaps it is an eGPU
                    (success, gpuId) = MetadataHandler.GetGpuIdFromName(gpu.name, !gpu.isNotebook);
                    
                    if (success) {
                        gpu.isNotebook = !gpu.isNotebook;
                        gpu.id = gpuId;
                    } else {
                        gpu.isValidated = false;
                    }
                }
            }

            int gpuCount = gpuList.Where(x => x.isValidated).Count();

            if (gpuCount > 0) {
                if (gpuCount > 1) {
                    // Validate that the GPU ID is still active on this system
                    int configGpuId = int.Parse(ConfigurationHandler.ReadSetting("GPU ID", gpuList));

                    foreach (GPU gpu in gpuList.Where(x => x.isValidated)) {
                        if (gpu.id == configGpuId) {
                            return (gpu, osId);
                        }
                    }

                    // GPU ID is no longer active on this system, prompt user to choose new GPU
                    configGpuId = int.Parse(ConfigurationHandler.SetupSetting("GPU ID", gpuList));

                    foreach (GPU gpu in gpuList.Where(x => x.isValidated)) {
                        if (gpu.id == configGpuId) {
                            return (gpu, osId);
                        }
                    }
                } else {
                    GPU gpu = gpuList.Where(x => x.isValidated).First();
                    return (gpu, osId);
                }
            }

            // If no GPU could be validated, then force recache once, and loop again.
            // This fixes issues related with outdated cache
            if (!forceRecache) {
                MetadataHandler.PrepareCache(true);
                return GetDriverMetadata(true);
            } else {
                Write("ERROR!");
                WriteLine();
                WriteLine("GPU metadata lookup has failed! Please file an issue on the GitHub project page and include the following information:");
                WriteLine();

                foreach (GPU gpu in gpuList)
                {
                    WriteLine($"GPU Name: '{gpu.name}' | VendorId: {gpu.vendorId} | DeviceId: {gpu.deviceId} | IsNotebook: {gpu.isNotebook}");
                }

                callExit(1);
                return (null, 0);
            }
        }

        private static JObject GetDriverDownloadInfo(int gpuId, int osId, bool isDchDriver) {
            try {
                var ajaxDriverLink = "https://gfwsl.geforce.com/services_toolkit/services/com/nvidia/services/AjaxDriverService.php?func=DriverManualLookup";
                ajaxDriverLink += $"&pfid={gpuId}&osID={osId}&dch={(isDchDriver ? 1 : 0)}";
                // Driver type (upCRD)
                // - 0 is Game Ready Driver (GRD)
                // - 1 is Studio Driver (SD)
                int driverTypeInt = ConfigurationHandler.ReadSetting("Driver type") == "grd" ? 0 : 1;
                ajaxDriverLink += $"&upCRD={driverTypeInt}";

                JObject driverObj = JObject.Parse(ReadURL(ajaxDriverLink));

                // Check if driver was found
                if ((int)driverObj["Success"] == 1) {

                    // If the operating system has support for DCH drivers, and DCH drivers are currently not installed, then serach for DCH drivers too.
                    // Non-DCH drivers are discontinued. Not searching for DCH drivers will result in users having outdated graphics drivers, and we don't want that.
                    if (Environment.Version.Build > 10240 && !isDchDriver) {
                        ajaxDriverLink = ajaxDriverLink[..^1] + "1";
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
                string driverType = ConfigurationHandler.ReadSetting("Driver type");

                Write("ERROR!");
                WriteLine();
                WriteLine("No NVIDIA driver was found for your system configuration.");
                WriteLine();
                WriteLine("Debugging information:");
                WriteLine($"gpuId:       {gpuId}");
                WriteLine($"osId:        {osId}");
                WriteLine($"isDchDriver: {isDchDriver}");
                WriteLine($"driverType:  {driverType}");

                // Ask user to switch to GRD driver
                if (driverType == "sd") {
                    WriteLine();
                    WriteLine("NOTICE: you have selected Studio Drivers (SD)");

                    TaskDialogButton[] buttons = [
                        new("Change to Game Ready Driver (GRD)") { Tag = "change" },
                        new("No") { Tag = "no" }
                    ];

                    string text = @"No driver was found for your system and you have choosen Studio Drivers." +
                        Environment.NewLine + Environment.NewLine +
                        "TNUC does currently not support searching for GRD and SD drivers at the same time." +
                        Environment.NewLine + Environment.NewLine +
                        "Do you wish to change driver type to Game Ready Drivers (GRD)?";

                    string result = ConfigurationHandler.ShowButtonDialog("Change driver type?", text, TaskDialogIcon.Warning, buttons);

                    if (result == "change") {
                        ConfigurationHandler.SetSetting("Driver type", "grd");
                        WriteLine("The driver type has now been changed to Game Ready Driver (GRD). Restart for changes to apply");
                    }
                }
            } catch (Exception ex) {
                Write("ERROR!");
                WriteLine();
                WriteLine("Unable to interact with NVIDIA API.");
                WriteLine();
                WriteLine(ex.ToString());
            }

            callExit(1);
            return null;
        }

        public static string ReadURL(string url)
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
            Write("Verifying internet connection . . . ");

            if (NetworkInterface.GetIsNetworkAvailable()) {
                Write("OK!");
                WriteLine();
            } else {
                Write("ERROR!");
                WriteLine();
                WriteLine("You are not connected to the internet!");
                callExit(2);
            }

            if (ConfigurationHandler.ReadSettingBool("Minimal install")) {
                if (LibraryHandler.EvaluateLibrary() == null) {
                    WriteLine("No compatible extract library was detected on the system. The minimal install feature has been disabled.");
                    ConfigurationHandler.SetSetting("Minimal install", "false");
                }
            }

            WriteLine();
        }

        /// <summary>
        /// Downloads the driver and some other stuff
        /// </summary>
        private static void DownloadDriver(string downloadURL)
        {
            DriverDialog.ShowGUI();

            if (DriverDialog.selectedBtn == DriverDialog.SelectedBtn.DLEXTRACT) {
                // download and save (and extract)
                WriteLine();

                string driverFileName = downloadURL.Split('/').Last(); // retrives file name from url
                string savePath = "";

                try {
                   string title = "Where do you want to save the driver?";

                    if (ConfigurationHandler.ReadSettingBool("Minimal install")) {
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
                        WriteLine("User closed dialog!");
                        return;
                    }

                    if (File.Exists(savePath + driverFileName) && !DoesDriverFileSizeMatch(savePath + driverFileName)) {
                        File.Delete(savePath + driverFileName);
                    }

                    // don't download driver if it already exists
                    Write("Downloading the driver . . . ");
                    if (showUI && !File.Exists(savePath + driverFileName)) {
                        HandleDownload(downloadURL, savePath + driverFileName).GetAwaiter().GetResult();
                    }
                    // show the progress bar gui
                    else if(!showUI && !File.Exists(savePath + driverFileName)) {
                        using DownloaderForm dlForm = new();
                        dlForm.DownloadFile(downloadURL, savePath + driverFileName);
                    }

                } catch (Exception ex) {
                    Write("ERROR!");
                    WriteLine();
                    WriteLine("Driver download failed.");
                    WriteLine();
                    WriteLine(ex.ToString());
                    WriteLine();
                    callExit(1);
                }

                Write("OK!");
                WriteLine();

                if (debug) {
                    WriteLine($"savePath: {savePath}");
                }

                if (ConfigurationHandler.ReadSettingBool("Minimal install")) {
                    MakeInstaller(false, savePath, driverFileName);
                }
            } else if (DriverDialog.selectedBtn == DriverDialog.SelectedBtn.DLINSTALL) {
                DownloadDriverQuiet(confirmDL, downloadURL);
            }
        }

        /// <summary>
        /// Downloads and installs the driver without user interaction
        /// </summary>
        private static void DownloadDriverQuiet(bool minimized, string downloadURL)
        {
            string driverFileName = downloadURL.Split('/').Last(); // retrives file name from url
            string savePath = Path.GetTempPath();

            string FULL_PATH_DIRECTORY = savePath + OnlineGPUVersion + @"\";
            string FULL_PATH_DRIVER = FULL_PATH_DIRECTORY + driverFileName;

            savePath = FULL_PATH_DIRECTORY;

            Directory.CreateDirectory(FULL_PATH_DIRECTORY);

            if (File.Exists(FULL_PATH_DRIVER) && !DoesDriverFileSizeMatch(FULL_PATH_DRIVER)) {
                File.Delete(savePath + driverFileName);
            }

            if (!File.Exists(FULL_PATH_DRIVER)) {
                Write("Downloading the driver . . . ");

                if (showUI || confirmDL) {
                    try {
                        HandleDownload(downloadURL, FULL_PATH_DRIVER).GetAwaiter().GetResult();

                        Write("OK!");
                        WriteLine();
                    } catch (Exception ex) {
                        Write("ERROR!");
                        WriteLine();
                        WriteLine(ex.ToString());
                        WriteLine();
                        callExit(1);
                    }
                } else {
                    using DownloaderForm dlForm = new();
                    dlForm.DownloadFile(downloadURL, FULL_PATH_DRIVER);
                }
            }

            bool minimalInstaller = ConfigurationHandler.ReadSettingBool("Minimal install");

            if (minimalInstaller) {
                MakeInstaller(minimized, FULL_PATH_DIRECTORY, driverFileName);
            }

            try {
                WriteLine();
                Write("Executing driver installer . . . ");

                string fileName = minimalInstaller ? FULL_PATH_DIRECTORY + "setup.exe" : FULL_PATH_DRIVER;

                ProcessStartInfo startInfo = new(fileName) {
                    UseShellExecute = true
                };

                if (minimized) {
                    startInfo.Arguments = "/s /noreboot";
                }

                Process.Start(startInfo).WaitForExit();
                Write("OK!");
            } catch (Exception ex) {
                WriteLine("An error occurred preventing the driver installer to execute!");
                WriteLine();
                WriteLine(ex.ToString());
                callExit(1);
            }

            WriteLine();

            try {
                Directory.Delete(FULL_PATH_DIRECTORY, true);
                WriteLine($"Cleaned up: {FULL_PATH_DIRECTORY}");
            } catch {
                WriteLine($"Could not cleanup: {FULL_PATH_DIRECTORY}");
            }

        }

        /// <summary>
        /// Shared method for the accual downloading of a file with the command line progress bar.
        /// </summary>
        /// <param name="url">URL path for download</param>
        /// <param name="path">Absolute file path</param>
        /// <returns></returns>
        async public static Task HandleDownload(string url, string path, EventHandler<float> progressHandle = null)
        {
            // if a partial file download exists, delete it now
            if (File.Exists(path)) {
                File.Delete(path);
            }

            path += ".part"; // add 'partial' to file name making it easier to identify as an incomplete download

            // if a partial file download exists, delete it now
            if (File.Exists(path)) {
                File.Delete(path);
            }

            HttpClient client = new();
            Progress<float> progress = new();
            Handlers.ProgressBar progressBar = null;

            if (progressHandle == null) {
                progressBar = new();

                progress.ProgressChanged += delegate (object sender, float progress) {
                    progressBar.Report(progress / 100);
                };
            } else {
                progress.ProgressChanged += progressHandle;
            }

            try {
                using (FileStream file = new(path, FileMode.Create, FileAccess.Write, FileShare.None))  {
                    await client.DownloadDataAsync(url, file, progress);
                }

                File.Move(path, path[..^5]); // rename back
                if (progressHandle == null ) { progressBar.Dispose(); }
            } catch {
                File.Delete(path);
                if (progressHandle == null) { progressBar.Dispose(); }
                throw;
            }
        }

        /// <summary>
        /// Remove telementry and only extract basic drivers
        /// </summary>
        private static void MakeInstaller(bool silent, string savePath, string fileName)
        {
            WriteLine();
            Write("Extracting drivers . . . ");

            LibraryFile libraryFile = LibraryHandler.EvaluateLibrary();
            using var process = new Process();
            LibraryHandler.Library library = libraryFile.LibraryName();

            // Extract full driver to then analyze
            if (library == LibraryHandler.Library.WINRAR) {
                process.StartInfo = new ProcessStartInfo {
                    FileName = libraryFile.GetInstallationDirectory() + "winrar.exe",
                    WorkingDirectory = savePath,
                    Arguments = $"x {fileName} -optemp -y",
                    UseShellExecute = false
                };

                if (silent) process.StartInfo.Arguments += " -ibck";
            } else if (library == LibraryHandler.Library.SEVENZIP) {
                process.StartInfo = new ProcessStartInfo {
                    WorkingDirectory = savePath,
                    Arguments = $"x {fileName} -otemp -y",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                if (silent) {
                    process.StartInfo.FileName = libraryFile.GetInstallationDirectory() + "7z.exe";
                } else {
                    process.StartInfo.FileName = libraryFile.GetInstallationDirectory() + "7zG.exe";
                }
            } else if (library == LibraryHandler.Library.NANAZIP) {
                process.StartInfo = new ProcessStartInfo {
                    WorkingDirectory = savePath,
                    Arguments = $"x {fileName} -otemp -y",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                if (silent) {
                    process.StartInfo.FileName = "NanaZipC.exe";
                } else {
                    process.StartInfo.FileName = "NanaZipG.exe";
                }
            }

            try {
                process.Start();
                process.WaitForExit();
            } catch (Exception ex) {
                Write("ERROR!");
                WriteLine();
                WriteLine(ex.ToString());
                callExit(1);
            }

            // Analyze with ComponentHandler
            List<Handlers.Component> components = ComponentHandler.ParseComponentData($"{savePath}temp");
            
            string textComponents = ConfigurationHandler.ReadSetting("Minimal install components", components, true);
            string[] arrayComponents = textComponents
                .Split(", ")
                .Select(s => s.Trim())
                .ToArray();

            string[] extractFiles = [.. arrayComponents, "NVI2", "EULA.txt", "license.txt", "ListDevices.txt", "setup.cfg", "setup.exe"];

            foreach (string file in extractFiles) {
                string filePath = Path.Combine(savePath, "temp", file);

                if (File.Exists(filePath))
                {
                    try
                    {
                        string destinationFilePath = Path.Combine(savePath, file);
                        File.Move(filePath, destinationFilePath);
                    }
                    catch { }
                }
                else if (Directory.Exists(filePath))
                {
                    try
                    {
                        string destinationDirectoryPath = Path.Combine(savePath, Path.GetFileName(filePath));
                        Directory.Move(filePath, destinationDirectoryPath);
                    }
                    catch { }
                }
            }

            Directory.Delete(Path.Combine(savePath, "temp"), true);

            // remove new EULA files from the installer config, or else the installer throws error codes
            // author https://github.com/cywq
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
            Write("OK!");
            WriteLine();
        }

        /// <summary>
        /// Intro with legal message, moved to reduce lines that ultimately does the same thing.
        /// </summary>
        private static void RunIntro()
        {
            if (!hasRunIntro) {
                hasRunIntro = true;
                //WriteLine($"TinyNvidiaUpdateChecker v{offlineVer} dev build");
                WriteLine($"TinyNvidiaUpdateChecker v{offlineVer}");
                WriteLine();
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
                WriteLine();
                WriteLine("Press any key to exit...");
            }

            if (showUI & !noPrompt) Console.ReadKey(true);
            FreeConsole();
            Environment.Exit(exitNum);
        }
        
        private static bool DoesDriverFileSizeMatch(string absoluteFilePath) {
            return new FileInfo(absoluteFilePath).Length == downloadFileSize;
        }

        public static void Write(string value = "")
        {
            if (!showUI) return;
            if (!consoleAttached) AttachConsole();
            if (debuggerAttached) AllocConsole();
            Console.Write(value);
        }

        public static void WriteLine(string value = "")
        {
            if (!showUI) return;
            if (!consoleAttached) AttachConsole();
            if (debuggerAttached) AllocConsole();
            Console.WriteLine(value);
        }

        private static void AttachConsole()
        {
            if (GetConsoleWindow() == IntPtr.Zero) {
                bool success = AttachConsole(ATTACH_PARENT_PROCESS);
                consoleAttached = true;

                if (success) {
                    WriteLine();
                } else {
                    AllocConsole();
                }
            }
        }
    }
}
