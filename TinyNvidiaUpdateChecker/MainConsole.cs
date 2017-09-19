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
using System.Configuration;
using System.Management;
using System.Net.NetworkInformation;

namespace TinyNvidiaUpdateChecker
{

    /*
    TinyNvidiaUpdateChecker - Check for NVIDIA GPU driver updates!
    Copyright (C) 2016-2017 Hawaii_Beach

    This program Is free software: you can redistribute it And/Or modify
    it under the terms Of the GNU General Public License As published by
    the Free Software Foundation, either version 3 Of the License, Or
    (at your option) any later version.

    This program Is distributed In the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty Of
    MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License For more details.

    You should have received a copy Of the GNU General Public License
    along with this program.  If Not, see <http://www.gnu.org/licenses/>.
    */

    class MainConsole
    {

        /// <summary>
        /// Server adress
        /// </summary>
        private readonly static string serverURL = "https://elpumpo.github.io/TinyNvidiaUpdateChecker/";

        /// <summary>
        /// Current client version
        /// </summary>
        private static string offlineVer = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        
        /// <summary>
        /// Remote client version
        /// </summary>
        private static string onlineVer;

        /// <summary>
        /// Current GPU driver version
        /// </summary>
        private static string OfflineGPUVersion;

        /// <summary>
        /// Remote GPU driver version
        /// </summary>
        private static string OnlineGPUVersion;

        /// <summary>
        /// Langauge ID for GPU driver download
        /// </summary>
        private static int langID;

        private static string downloadURL;
        private static string savePath;
        private static string driverFileName;
        private static string pdfURL;
        private static DateTime releaseDate;
        private static string releaseDesc;

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
        private static bool debug = false;

        /// <summary>
        /// Force a prompt to download GPU drivers
        /// </summary>
        private static bool forceDL = false;

        /// <summary>
        /// Will automaticly download and install drivers
        /// </summary>
        private static bool confirmDL = false;

        /// <summary>
        /// Direction for configuration folder, blueprint: <local-appdata><author><project-name>
        /// </summary>
        private static string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).CompanyName, FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName);
        
        public static string configFile = Path.Combine(configDir, "app.config");

        /// <summary>
        /// Has the intro been displayed? Because we do not want to display the intro multiple times.
        /// </summary>
        private static bool hasRunIntro = false;

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

            if (showUI == true) {
                AllocConsole();
            }

            ConfigInit();

            CheckDependencies();

            CheckWinVer();

            GetLanguage();

            string val = null;
            string key = "Check for Updates";

            while (val != "true" & val != "false") {
                val = SettingManager.ReadSetting(key); // refresh value each time

                if (val == "true") {
                    SearchForUpdates();
                    break;
                } else if (val == "false") {
                    break;
                } else {
                    // invalid value
                    SettingManager.SetupSetting(key);
                }   
            }

            GpuInfo();

            bool hasSelected = false;
            int iOffline = 0;
            try {
                iOffline = Convert.ToInt32(OfflineGPUVersion.Replace(".", string.Empty));
            } catch(Exception ex) {
                OfflineGPUVersion = "Unknown";
                Console.WriteLine(ex);
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
                        DownloadDriverQuiet();
                    } else {
                        DownloadDriver();
                    }
                }
            }


            if (!hasSelected) {
                if (forceDL) DownloadDriver();
            }

            Console.WriteLine();
            Console.WriteLine("Job done! Press any key to exit.");
            if (showUI == true) Console.ReadKey();
            LogManager.Log("BYE!", LogManager.Level.INFO);
            Environment.Exit(0);
        }

        /// <summary>
        /// Initialize configuration manager
        /// </summary>
        private static void ConfigInit()
        {
            // powered by the .NET framework "Settings" function

            // set config dir
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configFile);
            ResetConfigMechanism();

            if (debug == true) {
                Console.WriteLine("configFile: " + AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                Console.WriteLine();
            }
            LogManager.Log("configFile: " + configFile, LogManager.Level.INFO);

            // create config file
            if (!File.Exists(configFile)) {
                Console.WriteLine("Generating configuration file, this only happenes once.");

                SettingManager.SetupSetting("Check for Updates");
                SettingManager.SetupSetting("Show Driver Description");
                SettingManager.SetupSetting("GPU Name");
                SettingManager.SetupSetting("Minimal install");

                Console.WriteLine();
            }

        }

        /// <summary>
        /// Search for client updates
        /// </summary>
        private static void SearchForUpdates()
        {
            Console.Write("Searching for Updates . . . ");
            int error = 0;

            try {
                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument htmlDocument = htmlWeb.Load(serverURL);

                // get version
                HtmlNode tdVer = htmlDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == "currentVersion");
                onlineVer = tdVer.InnerText.Trim();

            } catch (Exception ex) {
                error++;
                onlineVer = "0.0.0";
                Console.Write("ERROR!");
                LogManager.Log(ex.Message, LogManager.Level.ERROR);
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
            }
            if (error == 0) {
                Console.Write("OK!");
                Console.WriteLine();
            }
           

            if (Convert.ToInt32(onlineVer.Replace(".", string.Empty)) > Convert.ToInt32(offlineVer.Replace(".", string.Empty))) {
                Console.WriteLine("There is a update available for TinyNvidiaUpdateChecker!");
                DialogResult dialog = MessageBox.Show("There is a new client update available to download, do you want to be navigate to the official GitHub download section?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialog == DialogResult.Yes) {
                    Process.Start("https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases");
                }
            }

            if (debug == true)
            {
                Console.WriteLine("offlineVer: " + offlineVer);
                Console.WriteLine("onlineVer:  " + onlineVer);
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
                winVer = "Unknown";
                string message = "You're running a non-supported version of Windows; the application will determine itself.";

                Console.WriteLine(message);
                Console.WriteLine("verOrg: " + verOrg);
                LogManager.Log(message, LogManager.Level.ERROR);
                if (showUI == true) Console.ReadKey();
                Environment.Exit(1);
            }

            if (debug == true) {
                Console.WriteLine("winVer: " + winVer);
                Console.WriteLine("osID:   " + osID.ToString());
                Console.WriteLine("verOrg: " + verOrg);
                Console.WriteLine();
            }

        }

        /// <summary>
        /// Handles the command line arguments </summary>
        /// <param name="theArgs"> Command line arguments in. Turned out that Environment.GetCommandLineArgs() wasn't any good.</param>
        private static void CheckArgs(string[] theArgs)
        {

            /// The command line argument handler does it's work here,
            /// for a list of available arguments, use the '--help' argument.

            foreach (var arg in theArgs)
            {

                // no window
                if (arg.ToLower() == "--quiet") {
                    FreeConsole();
                    showUI = false;
                }

                // erase config
                else if (arg.ToLower() == "--erase-config") {
                    if (File.Exists(configFile)) {
                        try {
                            File.Delete(configFile);
                        } catch (Exception ex) {
                            RunIntro();
                            Console.WriteLine(ex.StackTrace);
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
                    Console.WriteLine("Current version is " + offlineVer);
                    Console.WriteLine();
                }

                // automaticly download driver
                else if (arg.ToLower() == "--confirm-dl") {
                    confirmDL = true;
                }

                // help menu
                else if (arg.ToLower() == "--help") {
                    RunIntro();
                    Console.WriteLine("Usage: " + Path.GetFileName(Assembly.GetEntryAssembly().Location) + " [ARGS]");
                    Console.WriteLine();
                    Console.WriteLine("--quiet        Run application quiet.");
                    Console.WriteLine("--erase-config Erase local configuration file.");
                    Console.WriteLine("--debug        Enable debugging for extended information.");
                    Console.WriteLine("--force-dl     Force download of drivers.");
                    Console.WriteLine("--version      View version number.");
                    Console.WriteLine("--confirm-dl   Automaticly download and install driver if new one is available.");
                    Console.WriteLine("--help         Displays this message.");
                    Environment.Exit(0);
                }

                // unknown command, right?
                else
                {
                    RunIntro();
                    Console.WriteLine("Unknown command '" + arg + "', type --help for help.");
                    Console.WriteLine();
                }
            }
            
            // show the args if debug mode
            if (debug) {
                foreach (var arg in theArgs) {
                    RunIntro();
                    Console.WriteLine("Arg: " + arg);
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

            if (debug == true) {
                Console.WriteLine("langID:   " + langID);
                Console.WriteLine("cultName: " + cultName);
                Console.WriteLine();
            }
            LogManager.Log("langID: " + langID, LogManager.Level.INFO);
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

            // query local driver version
            try {
                while (string.IsNullOrEmpty(gpuName)) {
                    gpuName = SettingManager.ReadSetting("GPU Name");
                    if (string.IsNullOrEmpty(gpuName)) {
                        SettingManager.SetupSetting("GPU Name");
                    }
                }

                ManagementObjectSearcher objectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");

                // TODO: this is not the optimal code
                foreach (ManagementObject obj in objectSearcher.Get()) {
                    if(obj["Description"].ToString() == gpuName) {
                        OfflineGPUVersion = obj["DriverVersion"].ToString().Replace(".", string.Empty).Substring(5);
                        OfflineGPUVersion = OfflineGPUVersion.Substring(0, 3) + "." + OfflineGPUVersion.Substring(3); // add dot
                        break;
                    } else {
                       // gpu not found
                    }
                  
                }
            

            } catch (Exception ex) {
                error++;
                OfflineGPUVersion = "000.00";
                Console.Write("ERROR!");
                LogManager.Log(ex.ToString(), LogManager.Level.ERROR);
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
            }

            /// In order to proceed, we must input what GPU we have.
            /// Looking at the supported products on NVIDIA website for desktop and mobile GeForce series,
            /// we can see that they're sharing drivers with other GPU families, the only thing we have to do is tell the website
            /// if we're running a mobile or desktop GPU.
            
            int psID = 0;
            int pfID = 0;

            /// Get correct gpu drivers:
            /// you do not have to choose the exact GPU,
            /// looking at supported products, we see that the same driver package includes
            /// drivers for the majority GPU family.
            if (gpuName.Contains("M")) {
                // mobile | notebook
                psID = 99;  // GeForce 900M-series (M for Mobile)
                pfID = 758; // GTX 970M
            } else {
                // desktop
                psID = 98;  // GeForce 900-series
                pfID = 756; // GTX 970
            }

            // finish request
            try {
                gpuURL = "http://www.nvidia.com/Download/processDriver.aspx?psid=" + psID.ToString() + "&pfid=" + pfID.ToString() + "&rpf=1&osid=" + osID.ToString() + "&lid=" + langID.ToString() + "&ctk=0";

                WebClient client = new WebClient();
                Stream stream = client.OpenRead(gpuURL);
                StreamReader reader = new StreamReader(stream);
                processURL = reader.ReadToEnd();
                reader.Close();
                stream.Close();
            } catch (Exception ex) {
                if (error == 0) {
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    error++;
                }
                Console.WriteLine(ex.StackTrace);
            }

            try
            {
                // HTMLAgilityPack
                // thanks to http://www.codeproject.com/Articles/691119/Html-Agility-Pack-Massive-information-extraction-f for a great article

                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument htmlDocument = htmlWeb.Load(processURL);

                // get version
                HtmlNode tdVer = htmlDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == "tdVersion");
                OnlineGPUVersion = tdVer.InnerHtml.Trim().Substring(0, 6);

                // get release date
                HtmlNode tdReleaseDate = htmlDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == "tdReleaseDate");
                var dates = tdReleaseDate.InnerHtml.Trim();

                // not the best code, but does the job, might come back to cleanup in the future
                int status = 0;
                int year = 0;
                int month = 0;
                int day = 0;

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
                            LogManager.Log("The status: '" + status + "' is not a recognized status!", LogManager.Level.ERROR);
                            break;
                    }
                }            

                releaseDate = new DateTime(year, month, day); // follows the ISO 8601 standard 

                IEnumerable <HtmlNode> links = htmlDocument.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href"));
                foreach (var link in links) {

                    // get driver URL
                    if (link.Attributes["href"].Value.Contains("/content/DriverDownload-March2009/")) {
                        confirmURL = "http://www.nvidia.com" + link.Attributes["href"].Value.Trim();
                    }

                    // get release notes URL
                    if (link.Attributes["href"].Value.Contains("release-notes.pdf")) {
                        pdfURL = link.Attributes["href"].Value.Trim();
                    }
                    
                }

                if (pdfURL == null) {
                    if (psID == 98) { // if desktop
                        pdfURL = "http://us.download.nvidia.com/Windows/" + OnlineGPUVersion + "/" + OnlineGPUVersion + "-win10-win8-win7-desktop-release-notes.pdf";
                    } else {
                        pdfURL = "http://us.download.nvidia.com/Windows/" + OnlineGPUVersion + "/" + OnlineGPUVersion + "-win10-win8-win7-notebook-release-notes.pdf";
                    }
                    LogManager.Log("No release notes found, but a link to the notes has been crafted by following the template Nvidia uses.", LogManager.Level.INFO);
                }

                // get driver desc
                releaseDesc = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='tab1_content']").InnerHtml.Trim();
                releaseDesc = HtmlToText.ConvertHtml(releaseDesc + ".", gpuName.Contains("M"));

                // Remove not needed information
                if(psID == 98) { // desktop
                    releaseDesc = releaseDesc.Substring(297, releaseDesc.Length - 297).Trim();
                } else { // mobile
                    releaseDesc = releaseDesc.Substring(878, releaseDesc.Length - 878).Trim();
                }


                // get download link
                htmlDocument = htmlWeb.Load(confirmURL);
                links = htmlDocument.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href"));
                foreach (var link in links) {
                    if (link.Attributes["href"].Value.Contains("download.nvidia")) {
                        downloadURL = link.Attributes["href"].Value.Trim();
                        break; // don't need to keep search after we've found what we searched for
                    }
                }

            } catch (Exception ex) {
                OnlineGPUVersion = "000.00";
                LogManager.Log(ex.Message, LogManager.Level.ERROR);
                if (error == 0) {
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    error++;
                }
                Console.WriteLine(ex.StackTrace);
            }

            if (error == 0) {
                Console.Write("OK!");
                Console.WriteLine();
            }

            if (debug == true) {
                Console.WriteLine("gpuURL:      " + gpuURL);
                Console.WriteLine("processURL:  " + processURL);
                Console.WriteLine("confirmURL:  " + confirmURL);
                Console.WriteLine("downloadURL: " + downloadURL);
                Console.WriteLine("pdfURL:      " + pdfURL);
                Console.WriteLine("releaseDate: " + releaseDate.ToShortDateString());
                Console.WriteLine("OfflineGPUVersion: " + OfflineGPUVersion);
                Console.WriteLine("OnlineGPUVersion:  " + OnlineGPUVersion);
            }

        }

        /// <summary>
        /// Check if dependencies are all OK
        /// </summary>
        private static void CheckDependencies()
        {

            // Check internet connection
            Console.Write("Searching for a network connection . . . ");
            switch (NetworkInterface.GetIsNetworkAvailable()) {
                case true:
                    Console.Write("OK!");
                    Console.WriteLine();
                    break;

                default:
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    Console.WriteLine("No network connection was found, the application will now determinate!");
                    if (showUI == true) Console.ReadKey();
                    Environment.Exit(2);
                    break;
            }

            if (!File.Exists("HtmlAgilityPack.dll")) {

                Console.WriteLine();
                Console.Write("Attempting to download HtmlAgilityPack.dll . . . ");

                try {
                    using (WebClient webClient = new WebClient()) {
                        webClient.DownloadFile("https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases/download/v" + offlineVer + "/HtmlAgilityPack.dll", "HtmlAgilityPack.dll");
                    }
                    Console.Write("OK!");
                    Console.WriteLine();
                } catch (Exception ex) {
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine();
                }

            }

            string val = null;
            string key = "Minimal install";
            bool checkLib = false;

            // loop
            while (val != "true" & val != "false")
            {
                val = SettingManager.ReadSetting(key); // refresh value each time
                if (val == "true") {
                    checkLib = true;
                } else if (val == "false") {
                    break;
                } else {
                    // invalid value
                    SettingManager.SetupSetting(key);
                }
            }
            if(checkLib) {
                if(LibaryHandler.EvaluateLibary() == null) {
                    Console.WriteLine("Doesn't seem like either WinRAR or 7-Zip is installed!");
                    DialogResult dialogUpdates = MessageBox.Show("Do you want to disable the minimal install feature and use the traditional way?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dialogUpdates == DialogResult.Yes) {
                        SettingManager.SetSetting(key, "false");
                    } else {
                        Console.WriteLine("The application will determinate itself");
                        if (showUI == true) Console.ReadKey();
                        Environment.Exit(2);
                    }
                }
            }


            Console.WriteLine();
        }

        /// <summary>
        /// Downloads the driver and some other stuff
        /// </summary>
        private static void DownloadDriver()
        {
            int DateDiff = (DateTime.Now - releaseDate).Days; // how many days between the two dates
            string theDate = null;

            if (DateDiff == 1) {
                theDate = DateDiff + " day ago";
            } else if (DateDiff < 1) {
                theDate = "today"; // we only have the date and not time :/
            } else {
                theDate = DateDiff + " days ago";
            }

            string message = "Graphics card drivers are available, do you want to update now?" + Environment.NewLine + Environment.NewLine;

            string key = "Show Driver Description";
            string val = null;

            // loop
            while (val != "true" & val != "false") {
                val = SettingManager.ReadSetting(key); // refresh value each time

                if (val == "true") {
                    message = message + "Description: " + releaseDesc + Environment.NewLine + Environment.NewLine;
                } else if (val == "false") {
                    break;
                } else {
                    // invalid value
                    SettingManager.SetupSetting(key);
                }
            }

            message += "Driver version: " + OnlineGPUVersion + " (you're running " + OfflineGPUVersion + ")" + Environment.NewLine +
                        "Driver released: " + theDate + " (" + releaseDate.ToShortDateString() + ")";

            DialogResult dialog = MessageBox.Show(message, "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialog == DialogResult.Yes) {

                Console.WriteLine();

                bool error = false;
                driverFileName = downloadURL.Split('/').Last(); // retrives file name from url

                try {
                        
                    message = "Where do you want to save the drivers?";

                    key = "Minimal install";
                    val = null; // reset value

                    // loop
                    while (val != "true" & val != "false") {
                        val = SettingManager.ReadSetting(key); // refresh value each time
                        if (val == "true") {
                            message += " (you should select a empty folder)";
                        } else if (val == "false") {
                            break;
                        } else {
                            // invalid value
                            SettingManager.SetupSetting(key);
                        }
                    }

                    DialogResult result;
                    using (FolderBrowserDialog folderDialog = new FolderBrowserDialog()) {
                        folderDialog.Description = message;

                        result = folderDialog.ShowDialog(); // show dialog and get status (will wait for input)
                        switch (result) {
                            case DialogResult.OK:
                                savePath = folderDialog.SelectedPath + @"\";
                                break;

                            default:
                                Console.WriteLine("User closed dialog!");
                                return;
                        }
                    }

                    // don't download driver if it already exists
                    Console.Write("Downloading the driver . . . ");
                    if (showUI && !File.Exists(savePath + driverFileName)) {

                        using (WebClient webClient = new WebClient()) {
                            var notifier = new AutoResetEvent(false);
                            var progress = new ProgressBar();

                            webClient.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e)
                            {
                                progress.Report((double)e.ProgressPercentage / 100);

                                if (e.BytesReceived >= e.TotalBytesToReceive) notifier.Set();
                            };

                            webClient.DownloadFileAsync(new Uri(downloadURL), savePath + driverFileName);

                            notifier.WaitOne(); // sync with the above
                            progress.Dispose(); // get rid of the progress bar
                        }
                    }
                    // show the progress bar gui
                    else if(!showUI && !File.Exists(savePath + driverFileName)) {
                        DownloaderForm dlForm = new DownloaderForm();
                        
                        dlForm.Show();
                        dlForm.Focus();
                        dlForm.DownloadFile(new Uri(downloadURL), savePath + driverFileName);
                        dlForm.Close();
                    }
                    else {
                        LogManager.Log("Driver is already downloaded", LogManager.Level.INFO);
                    }

                }
                catch (Exception ex)
                {
                    error = true;
                    Console.Write("ERROR!");
                    LogManager.Log(ex.Message, LogManager.Level.ERROR);
                    Console.WriteLine();
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine();
                }

                if (error == false)
                {
                    Console.Write("OK!");
                    Console.WriteLine();
                }

                if (debug == true) {
                    Console.WriteLine("savePath: " + savePath);
                }

                dialog = MessageBox.Show("Do you want view the release PDF?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialog == DialogResult.Yes) {
                    try {
                        Process.Start(pdfURL);
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex.StackTrace);
                    }
                }

                key = "Minimal install";
                val = null; // reset value
                // loop
                while (val != "true" & val != "false") {
                    val = SettingManager.ReadSetting(key); // refresh value each time
                    if (val == "true") {
                        MakeInstaller(false);
                    } else if (val == "false") {
                        break;
                    } else {
                        // invalid value
                        SettingManager.SetupSetting(key);
                    }
                }

                dialog = MessageBox.Show("Do you wish to run the driver installer?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialog == DialogResult.Yes) {
                    try {
                        if(val == "true") {
                            // extracted
                            Process.Start(savePath + "setup.exe");
                        } else {
                            Process.Start(savePath + driverFileName);
                        }
                        
                    } catch (Exception ex) {
                        Console.WriteLine(ex.StackTrace);
                    }
                    
                }
            }
        }

        /// <summary>
        /// Downloads and installs the driver without user interaction
        /// </summary>
        private static void DownloadDriverQuiet()
        {
            driverFileName = downloadURL.Split('/').Last(); // retrives file name from url
            savePath = Path.GetTempPath();

            string FULL_PATH_DIRECTORY = savePath + OnlineGPUVersion + @"\";
            string FULL_PATH_DRIVER = FULL_PATH_DIRECTORY + driverFileName;

            savePath = FULL_PATH_DIRECTORY;

            Directory.CreateDirectory(FULL_PATH_DIRECTORY);

            Console.WriteLine("FULL_PATH_DIRECTORY: " + FULL_PATH_DIRECTORY);
            Console.WriteLine("FULL_PATH_DRIVER: " + FULL_PATH_DRIVER);

            if (!File.Exists(FULL_PATH_DRIVER)) {
                using (WebClient webClient = new WebClient()) {
                    webClient.DownloadFile(downloadURL, FULL_PATH_DRIVER);
                }
            }

            MakeInstaller(true);

            try {
                Console.Write("Running installer . . . ");
                Process.Start(FULL_PATH_DIRECTORY + "setup.exe", "/s").WaitForExit();
            } catch {
                Console.WriteLine("Could not run driver installer!");
            } finally {
                Console.Write("OK!");
                Console.WriteLine();
            }
            
            try {
                Directory.Delete(FULL_PATH_DIRECTORY);
            } catch {
                
            } finally {
                Console.WriteLine("cleaned up: " + FULL_PATH_DIRECTORY);
            }

        }

        /// <summary>
        /// Remove telementry and only extract basic drivers
        /// </summary>
        private static void MakeInstaller(bool silent)
        {
            Console.WriteLine();
            Console.Write("Making installer . . . ");

            LibaryFile libaryFile = LibaryHandler.EvaluateLibary();

            string[] filesToExtract = { "Display.Driver", "NVI2", "EULA.txt", "license.txt", "ListDevices.txt", "setup.cfg", "setup.exe" };

            File.WriteAllLines(savePath + "inclList.txt", filesToExtract);

            string fullDriverPath = @"""" + savePath + driverFileName + @"""";

            if (libaryFile.libary == LibaryHandler.Libary.WINRAR) {
                using (Process WinRAR = new Process()) {
                    WinRAR.StartInfo.FileName = libaryFile.InstallLocation + "winrar.exe";
                    WinRAR.StartInfo.WorkingDirectory = savePath;
                    WinRAR.StartInfo.Arguments = "X " + fullDriverPath + @" -N@""inclList.txt""";
                    if (silent) WinRAR.StartInfo.Arguments += " -ibck -y";
                    WinRAR.StartInfo.UseShellExecute = false;
                    WinRAR.Start();
                    WinRAR.WaitForExit();
                }
            } else if (libaryFile.libary == LibaryHandler.Libary.SEVENZIP) {
                using (Process SevenZip = new Process()) {
                    if (silent) {
                        SevenZip.StartInfo.FileName = libaryFile.InstallLocation + "7z.exe";
                    } else {
                        SevenZip.StartInfo.FileName = libaryFile.InstallLocation + "7zG.exe";
                    }
                    SevenZip.StartInfo.WorkingDirectory = savePath;
                    SevenZip.StartInfo.Arguments = "x " + fullDriverPath + @" @inclList.txt";
                    if (silent) SevenZip.StartInfo.Arguments += " -y";
                    SevenZip.StartInfo.UseShellExecute = false;
                    SevenZip.StartInfo.CreateNoWindow = true; // don't show the console in our console!
                    SevenZip.Start();
                    SevenZip.WaitForExit();
                }
            }

            Console.Write("OK!");
            Console.WriteLine();

        }

        /// <summary>
        /// Intro with legal message, moved to reduce lines that ultimately does the same thing.
        /// </summary>
        private static void RunIntro()
        {
            if(!hasRunIntro) {
                hasRunIntro = true;
                Console.WriteLine("TinyNvidiaUpdateChecker v" + offlineVer + " dev build");
               // Console.WriteLine("TinyNvidiaUpdateChecker v" + offlineVer);
                Console.WriteLine();
                Console.WriteLine("Copyright (C) 2016-2017 Hawaii_Beach");
                Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY");
                Console.WriteLine("This is free software, and you are welcome to redistribute it");
                Console.WriteLine("under certain conditions. Licensed under GPLv3.");
                Console.WriteLine();
            } else {
                LogManager.Log("Intro has already been run!", LogManager.Level.INFO);
            }
        }

        /// <summary>
        /// Credit goes to Daniel Hilgarth for the weird bug fix I'm experiencing on my dev station, call after setting config dir.
        /// </summary>
        private static void ResetConfigMechanism()
        {
            typeof(ConfigurationManager)
            .GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, 0);

            typeof(ConfigurationManager)
            .GetField("s_configSystem", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, null);

            typeof(ConfigurationManager)
            .Assembly.GetTypes()
            .Where(x => x.FullName ==
               "System.Configuration.ClientConfigPaths")
            .First()
            .GetField("s_current", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, null);
        }

    }
}