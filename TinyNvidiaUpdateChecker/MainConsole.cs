using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Reflection;
using HtmlAgilityPack;
using System.Threading;
using System.Management;
using System.Net.NetworkInformation;
using System.ComponentModel;
using System.Xml;
using TinyNvidiaUpdateChecker.Handlers;
using Microsoft.Win32;

namespace TinyNvidiaUpdateChecker
{

    class MainConsole
    {

        /// <summary>
        /// Server adress
        /// </summary>
        private readonly static string serverURL = "https://elpumpo.github.io/TinyNvidiaUpdateChecker/";

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

        /// <summary>
        /// Langauge ID for GPU driver download
        /// </summary>
        private static int langID;

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
        /// Local Windows version
        /// </summary>
        private static string winVer;

        /// <summary>
        /// OS ID for GPU driver download
        /// </summary>
        private static int osID;

        /// <summary>
        /// Show UI or go quiet mode
        /// </summary>
        public static bool showUI = true;

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
        /// Should the application use the working directory as the path for the config file?
        /// </summary>
        public static bool configSwitch = false;

        /// <summary>
        /// Has the intro been displayed? Because we do not want to display the intro multiple times.
        /// </summary>
        private static bool hasRunIntro = false;

        /// <summary>
        /// Should we ignore that no compatible gpu were found?
        /// </summary>
        private static bool ignoreMissingGpu = false;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [STAThread]
        private static void Main(string[] args)
        {
            string message = "TinyNvidiaUpdateChecker v" + offlineVer;
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

            SettingManager.ConfigInit();

            CheckDependencies();

            CheckWinVer();

            GetLanguage();

            if (SettingManager.ReadSettingBool("Check for Updates")) {
                SearchForUpdates();
            }

            GpuInfo();

            bool hasSelected = false;
            int iOffline = 0;

            try {
                iOffline = Convert.ToInt32(OfflineGPUVersion.Replace(".", string.Empty));
            } catch(Exception ex) {
                OfflineGPUVersion = "Unknown";
                Console.WriteLine("Could not retrive OfflineGPUVersion!");
                Console.WriteLine(ex.ToString());
            }

            int iOnline = Convert.ToInt32(OnlineGPUVersion.Replace(".", string.Empty));

            if (iOnline == iOffline) {
                Console.WriteLine("Your GPU drivers are up-to-date!");
            } else {
                if (iOffline > iOnline) {
                    Console.WriteLine("Your current GPU driver is newer than remote!");
                } if (iOnline < iOffline) {
                    Console.WriteLine("Your GPU drivers are up-to-date!");
                } else {
                    Console.WriteLine("There are new drivers available to download!");
                    hasSelected = true;

                    if (confirmDL) {
                        DownloadDriverQuiet(true);
                    } else {
                        DownloadDriver();
                    }
                }
            }

            if (!hasSelected && forceDL) {
                if (confirmDL) {
                    DownloadDriverQuiet(true);
                } else {
                    DownloadDriver();
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            if (showUI) Console.ReadKey();
            LogManager.Log("BYE!", LogManager.Level.INFO);
            Environment.Exit(0);
        }

        /// <summary>
        /// Search for client updates
        /// </summary>
        private static void SearchForUpdates()
        {
            Console.Write("Searching for Updates . . . ");
            bool error = false;

            try {
                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument htmlDocument = htmlWeb.Load(serverURL);

                // get version
                HtmlNode tdVer = htmlDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == "currentVersion");
                onlineVer = tdVer.InnerText.Trim();

            } catch (Exception ex) {
                error = true;
                onlineVer = "0.0.0";
                Console.Write("ERROR!");
                LogManager.Log(ex.ToString(), LogManager.Level.ERROR);
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
            }
            if (!error) {
                Console.Write("OK!");
                Console.WriteLine();
            }

            if (new Version(onlineVer).CompareTo(new Version(offlineVer)) > 0) {
                Console.WriteLine("There is a update available for TinyNvidiaUpdateChecker!");

                if(!confirmDL) {
                    DialogResult dialog = MessageBox.Show("There is a new client update available to download, do you want to be navigate to the official GitHub download section?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialog == DialogResult.Yes) {
                        Process.Start("https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases");
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
        /// Gets the current Windows version and sets important value 'osID'.
        /// </summary>
        /// <seealso cref="GpuInfo"> Used here, decides OS and OS architecture.</seealso>
        private static void CheckWinVer()
        {
            string verOrg = Environment.OSVersion.Version.ToString();
            Boolean is64 = Environment.Is64BitOperatingSystem;

            if (verOrg.Contains("10.0")) {
                winVer = "10";
                if (is64) {
                    osID = 57;
                } else {
                    osID = 56;
                }
            }

            // Windows 8.1
            else if (verOrg.Contains("6.3")) {
                winVer = "8.1";
                if (is64) {
                    osID = 41;
                } else {
                    osID = 40;
                }
            }

            // Windows 8
            else if (verOrg.Contains("6.2")) {
                winVer = "8";
                if (is64) {
                    osID = 28;
                } else {
                    osID = 27;
                }
            }

            // Windows 7
            else if (verOrg.Contains("6.1")) {
                winVer = "7";
                if (is64) {
                    osID = 19;
                } else {
                    osID = 18;
                }

            } else {
                Console.WriteLine("You are not running a supported version of Windows!");
                Console.WriteLine($"verOrg: {verOrg}");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                if (showUI) Console.ReadKey();
                Environment.Exit(1);
            }

            if (debug) {
                Console.WriteLine($"winVer: {winVer}");
                Console.WriteLine($"osID:   {osID.ToString()}");
                Console.WriteLine($"verOrg: {verOrg}");
                Console.WriteLine();
            }

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

                // change the config path to the same path as application
                else if (arg.ToLower() == "--config-here") {
                    configSwitch = true;
                }

                // ignore incompatible gpu
                else if (arg.ToLower() == "--ignore-missing-gpu") {
                    ignoreMissingGpu = true;
                }

                // help menu
                else if (arg.ToLower() == "--help") {
                    RunIntro();
                    Console.WriteLine($"Usage: {Path.GetFileName(Assembly.GetEntryAssembly().Location)} [ARGS]");
                    Console.WriteLine();
                    Console.WriteLine("--quiet               Runs the application quietly in the background, and will only notify the user if an update is available.");
                    Console.WriteLine("--erase-config        Erase local configuration file.");
                    Console.WriteLine("--debug               Turn debugging on, will output more information that can be used for debugging.");
                    Console.WriteLine("--force-dl            Force prompt to download drivers, even if the user is up-to-date - should only be used for debugging.");
                    Console.WriteLine("--version             View version number.");
                    Console.WriteLine("--confirm-dl          Automatically download and install the driver quietly without any user interaction at all. should be used with '--quiet' for the optimal solution.");
                    Console.WriteLine("--config-here         Use the working directory as path to the config file.");
                    Console.WriteLine("--ignore-missing-gpu  Ignore the fact that no compatible were found.");
                    Console.WriteLine("--help                Displays this message.");
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
        /// Gets the local langauge used by operator and sets value 'langID'.
        /// </summary>
        /// <seealso cref="GpuInfo"> Used here, decides driver download language and possibly download server.</seealso>
        private static void GetLanguage()
        {
            string cultName = CultureInfo.CurrentCulture.ToString(); // https://msdn.microsoft.com/en-us/library/ee825488(v=cs.20).aspx - http://www.lingoes.net/en/translator/langcode.htm

            switch (cultName)
            {
                case "en-US":  // English - United States
                    langID = 1;
                    break;
                case "en-GB":  // English - United Kingdom
                    langID = 2;
                    break;
                case "zh-CHS": // Chinese (Simplified)
                    langID = 5;
                    break;
                case "zh-CHT": // Chinese (Traditional)
                    langID = 6;
                    break;
                case "ja-JP":  // Japanese - Japan
                    langID = 7;
                    break;
                case "ko-KR":  // Korean - Korea
                    langID = 8;
                    break;
                case "de-DE":  // German - Germany
                    langID = 9;
                    break;
                case "es-ES":  // Spanish - Spain
                    langID = 10;
                    break;
                case "fr-FR":  // French - France
                    langID = 12;
                    break;
                case "it-IT":  // Italian - Italy
                    langID = 13;
                    break;
                case "pl-PL":  // Polish - Poland
                    langID = 14;
                    break;
                case "pt-BR":  // Portuguese - Brazil
                    langID = 15;
                    break;
                case "ru-RU":  // Russian - Russia
                    langID = 16;
                    break;
             /* case "tr-TR":  // Turkish - Turkey
                    langID = 19;
                    break;
             */
                default:
                    // intl
                    langID = 17;
                    break;
            }

            if (debug) {
                Console.WriteLine($"langID:   {langID}");
                Console.WriteLine($"cultName: {cultName}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// A lot of things going on inside: gets current gpu driver, fetches latest gpu driver from NVIDIA server and fetches download link for latest drivers.
        /// </summary>
        private static void GpuInfo()
        {
            Console.Write("Retrieving GPU information . . . ");
            int error = 0;
            string processURL = null;
            string confirmURL = null;
            string gpuURL = null;
            string gpuName = null;
            bool foundGpu = false;

            // query local driver version
            try {
                foreach (ManagementObject obj in new ManagementObjectSearcher("SELECT * FROM Win32_VideoController").Get()) {
                    if (obj["Description"].ToString().ToLower().Contains("nvidia")) {
                        gpuName = obj["Description"].ToString().Trim();

                        var extractedDriver = obj["DriverVersion"].ToString().Replace(".", string.Empty);
                        OfflineGPUVersion = extractedDriver.Substring(extractedDriver.Length - 5, 5);
                        OfflineGPUVersion = OfflineGPUVersion.Substring(0, 3) + "." + OfflineGPUVersion.Substring(3); // add dot
                        foundGpu = true;
                        break;
                    } else if (obj["PNPDeviceID"].ToString().ToLower().Contains("ven_10de")) {
                        foreach (ManagementObject obj1 in new ManagementClass("Win32_SystemEnclosure").GetInstances()) {
                            foreach (int chassisType in (UInt16[])(obj1["ChassisTypes"])) {
                                gpuName = (chassisType == 3) ? "GTX" : "GTX M";
                            }
                        }

                        foundGpu = true;
                        break;
                    } else { // gpu not found
                        LogManager.Log(obj["Description"].ToString().Trim() + " is not NVIDIA!", LogManager.Level.INFO);
                    }
                }

                if (!foundGpu) {
                    if (ignoreMissingGpu) {
                        gpuName = "GTX";
                    } else {
                        throw new InvalidDataException();
                    }
                }

            } catch (InvalidDataException) {
                Console.Write("ERROR!");
                Console.WriteLine();
                Console.WriteLine("No supported nvidia GPU was found!");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                if (showUI) Console.ReadKey();
                Environment.Exit(1);
            } catch (Exception ex) {
                error++;
                OfflineGPUVersion = "000.00";
                Console.Write("ERROR!");
                LogManager.Log(ex.ToString(), LogManager.Level.ERROR);
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
            }

            /// In order to proceed, we must input what GPU we have.
            /// Looking at the supported products on NVIDIA website for desktop and mobile GeForce series,
            /// we can see that they're sharing drivers with other GPU families, the only thing we have to do is tell the website
            /// if we're running a mobile or desktop GPU.

            int psID = 0, pfID = 0;

            /// Get correct gpu drivers:
            /// you do not have to choose the exact GPU,
            /// looking at supported products, we see that the same driver package includes
            /// drivers for the majority GPU family.
            if (gpuName.Contains("M")) { // mobile | notebook
                psID = 99;  // GeForce 900M-series (M for Mobile)
                pfID = 758; // GTX 970M
            } else { // desktop
                psID = 98;  // GeForce 900-series
                pfID = 756; // GTX 970
            }

            // finish request
            try {
                gpuURL = $"https://www.nvidia.com/Download/processDriver.aspx?psid={psID}&pfid={pfID}&osid={osID}&lid={langID}&dtcid=1&ctk=0";
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(gpuURL);
                StreamReader reader = new StreamReader(stream);
                processURL = reader.ReadToEnd();
                processURL = "https:" + processURL; // NVIDIA broke this around 2022-10 so this is a quick fix until I rewrite this code
                reader.Close();
                stream.Close();
            } catch (Exception ex) {
                if (error == 0) {
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    error++;
                }
                Console.WriteLine(ex.ToString());
            }

            try
            {
                // HTMLAgilityPack
                // thanks to http://www.codeproject.com/Articles/691119/Html-Agility-Pack-Massive-information-extraction-f for a great article

                var htmlWeb = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument htmlDocument = htmlWeb.Load(processURL);

                // get version
                var tdVer = htmlDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == "tdVersion");
                OnlineGPUVersion = tdVer.InnerHtml.Trim().Substring(0, 6);

                // get release date
                var tdReleaseDate = htmlDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == "tdReleaseDate");
                var dates = tdReleaseDate.InnerHtml.Trim();

                // get driver release date
                int status = 0, year = 0, month = 0, day = 0;

                foreach (var substring in dates.Split('.')) {
                    status++; // goes up starting from 1, being the year, followed by month then day.
                    switch(status) {

                        // year
                        case 1:
                            year = Convert.ToInt32(substring);
                            break;

                        // month
                        case 2:
                            month = Convert.ToInt32(substring);
                            break;

                        // day
                        case 3:
                            day = Convert.ToInt32(substring);
                            break;

                        default:
                            LogManager.Log($"The status: '{status}' is not a recognized status!", LogManager.Level.ERROR);
                            break;
                    }
                }

                releaseDate = new DateTime(year, month, day);
                IEnumerable <HtmlNode> node = htmlDocument.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href"));

                // get driver URL
                foreach (var child in node) {
                    if (child.Attributes["href"].Value.Contains("/content/DriverDownloads/")) {
                        confirmURL = "https://www.nvidia.com" + child.Attributes["href"].Value.Trim();
                        break;
                    }
                }

                // get release notes URL
                foreach (var child in node) {
                    if (child.Attributes["href"].Value.Contains("release-notes.pdf")) {
                        pdfURL = child.Attributes["href"].Value.Trim();
                        break;
                    }
                }

                if (pdfURL == null) {
                    pdfURL = $"https://us.download.nvidia.com/Windows/{OnlineGPUVersion}/{OnlineGPUVersion}-win11-win10-win8-win7-release-notes.pdf";
                    LogManager.Log("No release notes found, but a link to the notes has been crafted by following the template Nvidia uses.", LogManager.Level.INFO);
                }

                // get driver description and show it in HTML
                releaseDesc = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='tab1_content']").InnerHtml.Trim();

                // get download link
                htmlDocument = htmlWeb.Load(confirmURL);
                node = htmlDocument.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href"));

                foreach (var child in node) {
                    if (child.Attributes["href"].Value.Contains("download.nvidia")) {
                        downloadURL = child.Attributes["href"].Value.Trim();
                        break;
                    }
                }

                var locationPrefix = SettingManager.ReadSetting("Download location");
                downloadURL = downloadURL.Substring(4);
                downloadURL = $"https://{locationPrefix}{downloadURL}";

                // get file size
                using (var responce = WebRequest.Create(downloadURL).GetResponse()) {
                    downloadFileSize = responce.ContentLength;
                }

            } catch (Exception ex) {
                OnlineGPUVersion = "000.00";
                LogManager.Log(ex.ToString(), LogManager.Level.ERROR);
                if (error == 0) {
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    error++;
                }
                Console.WriteLine(ex.ToString());
            }

            if (error == 0) {
                Console.Write("OK!");
                Console.WriteLine();
            }

            if (debug) {
                Console.WriteLine($"gpuURL: {gpuURL}");
                Console.WriteLine($"downloadURL: {downloadURL}");
                Console.WriteLine($"pdfURL:      {pdfURL}");
                Console.WriteLine($"releaseDate: {releaseDate.ToShortDateString()}");
                Console.WriteLine($"downloadFileSize:  {Math.Round((downloadFileSize / 1024f) / 1024f)} MB ({downloadFileSize:N} bytes)");
                Console.WriteLine($"OfflineGPUVersion: {OfflineGPUVersion}");
                Console.WriteLine($"OnlineGPUVersion:  {OnlineGPUVersion}");
            }

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
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                if (showUI) Console.ReadKey();
                Environment.Exit(2);
            }

            var workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var hapPath = Path.Combine(workingDirectory, "HtmlAgilityPack.dll");

            if (File.Exists(hapPath)) {
                Console.WriteLine();
                Console.Write("Verifying HAP MD5 hash . . . ");

                var hash = HashHandler.CalculateMD5(hapPath);

                if (hash.md5 != HashHandler.HAP_HASH && hash.error == false) {
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    Console.WriteLine("Deleting the invalid HAP file.");

                    try {
                        File.Delete(hapPath);
                    } catch (Exception ex) {
                        Console.WriteLine(ex.ToString());
                    }

                // delete HAP file as it couldn't be verified
                } else if (hash.error) {
                    try {
                        File.Delete(hapPath);
                    } catch (Exception ex) {
                        Console.WriteLine(ex.ToString());
                    }
                } else {
                    Console.Write("OK!");
                    Console.WriteLine();
                }

                if (debug) {
                    Console.WriteLine($"Generated hash: {hash.md5}");
                    Console.WriteLine($"Known hash:     {HashHandler.HAP_HASH}");
                }
            }

            if (!File.Exists(hapPath)) {
                Console.WriteLine();
                Console.Write("Attempting to download HtmlAgilityPack.dll . . . ");

                try {
                    using (var webClient = new WebClient()) {
                        webClient.DownloadFile($"https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases/download/v{offlineVer}/HtmlAgilityPack.dll", hapPath);
                    }

                    Console.Write("OK!");
                    Console.WriteLine();
                } catch (Exception ex) {
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine();
                }
            }

            // compare HAP version, too
            var currentHapVersion = AssemblyName.GetAssemblyName(hapPath).Version.ToString();

            if (new Version(HashHandler.HAP_VERSION).CompareTo(new Version(currentHapVersion)) != 0) {
                Console.WriteLine($"ERROR: The current HAP libary v{currentHapVersion} does not match the required v{HashHandler.HAP_VERSION}");
                Console.WriteLine("The application will not continue to prevent further errors");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                if (showUI) Console.ReadKey();
                Environment.Exit(1);
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
                   string message = "Where do you want to save the drivers?";

                    if (SettingManager.ReadSettingBool("Minimal install")) {
                        message += " (you should select a empty folder)";
                    }

                    var folderSelectDialog = new FolderSelectDialog();
                    folderSelectDialog.Title = message;

                    if (folderSelectDialog.Show()) {
                        savePath = folderSelectDialog.FileName + @"\";
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
                        using (DownloaderForm dlForm = new DownloaderForm()) {
                            dlForm.Show();
                            dlForm.Focus();
                            dlForm.DownloadFile(new Uri(downloadURL), savePath + driverFileName);
                            dlForm.Close();
                        }
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
                    using (DownloaderForm dlForm = new DownloaderForm()) {
                        dlForm.Show();
                        dlForm.Focus();
                        dlForm.DownloadFile(new Uri(downloadURL), FULL_PATH_DRIVER);
                        dlForm.Close();
                    }
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

                Process.Start(SettingManager.ReadSettingBool("Minimal install") ? FULL_PATH_DIRECTORY + "setup.exe" : FULL_PATH_DRIVER, arguments).WaitForExit();
                Console.Write("OK!");
            } catch {
                Console.WriteLine("An error occurred preventing the driver installer to execute!");
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
                using (var webClient = new WebClient()) {
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
                }
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
                using (var WinRAR = new Process()) {
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

                }
            } else if (libaryFile.LibaryName() == LibaryHandler.Libary.SEVENZIP) {
                using (var SevenZip = new Process()) {
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

        private static bool DoesDriverFileSizeMatch(string FULL_PATH_DRIVER)
        {
            return new FileInfo(FULL_PATH_DRIVER).Length == downloadFileSize;
        }
    }
}
