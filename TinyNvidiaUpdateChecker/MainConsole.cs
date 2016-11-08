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

namespace TinyNvidiaUpdateChecker
{

    /*
    TinyNvidiaUpdateChecker - Check for NVIDIA GPU drivers, GeForce Experience replacer
    Copyright (C) 2016 Hawaii_Beach

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
        private static string pdfURL;
        private static DateTime releaseDate;

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
        /// Force a download of GPU drivers
        /// </summary>
        private static bool forceDL = false;

        /// <summary>
        /// Direction for configuration folder, blueprint: <local-appdata><author><project-name>
        /// </summary>
        private static string dirToConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).CompanyName, FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName);
        
        public static string fullConfig = Path.Combine(dirToConfig, "app.config");

        /// <summary>
        /// Has the intro been displayed? Because we do not want to display the intro multiple times.
        /// </summary>
        private static bool HasIntro = false;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string pszPath);

        [STAThread]
        private static void Main(string[] args)
        {
            string message = "TinyNvidiaUpdateChecker v" + offlineVer;
            LogManager.log(message, LogManager.Level.INFO);
            Console.Title = message;

            CheckArgs(args);

            RunIntro(); // will run intro if no args needs to output stuff

            if (showUI == true)
            {
                AllocConsole();

                // disable CTRL+C
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                };
            }

            checkDll();

            configInit();

            checkWinVer();

            getLanguage();
            
            bool set = false;
            string key = "Check for Updates";

            while (set == false) {
                string val = SettingManager.readSetting(key); // refresh value each time

                if (val == "true") {
                    searchForUpdates();
                    set = true;
                } else if (val == "false") {
                    set = true; // leave loophole
                } else {
                    // invalid value
                    SettingManager.setupSetting(key);
                }   
            }

            gpuInfo();

            Boolean hasSelected = false;
            int iOffline = Convert.ToInt32(OfflineGPUVersion.Replace(".", string.Empty));
            int iOnline = Convert.ToInt32(OnlineGPUVersion.Replace(".", string.Empty));

            if (iOnline == iOffline) {
                Console.WriteLine("Your GPU drivers are up-to-date!");
            } else {
                if (iOffline > iOnline) {
                    Console.WriteLine("Your current GPU driver is newer than remote!");}
                if (iOnline < iOffline) {
                    Console.WriteLine("Your GPU drivers are up-to-date!");
                } else {
                    Console.WriteLine("There are new drivers available to download!");
                    hasSelected = true;
                    downloadDriver();
                }
            }

            if (hasSelected == false)
            {
                if (forceDL == true) downloadDriver();
            }
            

            Console.WriteLine();
            
            Console.WriteLine("Job done! Press any key to exit.");
            if (showUI == true) Console.ReadKey();
            LogManager.log("BYE!", LogManager.Level.INFO);
            Environment.Exit(0);
        }

        /// <summary>
        /// Initialize configuration manager
        /// </summary>
        private static void configInit()
        {
            // powered by the .NET framework "Settings" function

            // set config dir
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", fullConfig);

            if (debug == true) {
                Console.WriteLine("Current configuration file is located at: " + AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                Console.WriteLine();
            }
            LogManager.log("ConfigDir: " + fullConfig, LogManager.Level.INFO);

            // create config file
            if (!File.Exists(fullConfig)) {
                Console.WriteLine("Generating configuration file, this only happenes once.");

                SettingManager.setupSetting("Check for Updates");
                SettingManager.setupSetting("GPU Type");

                Console.WriteLine();
            }

        }

        /// <summary>
        /// Search for client updates
        /// </summary>
        private static void searchForUpdates()
        {
            Console.Write("Searching for Updates . . . ");
            int error = 0;
            try
            {
                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument htmlDocument = htmlWeb.Load(serverURL);

                // get version
                HtmlNode tdVer = htmlDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == "currentVersion");
                onlineVer = tdVer.InnerText.Trim();

            } catch (Exception ex) {
                error++;
                onlineVer = "0.0.0";
                Console.Write("ERROR!");
                LogManager.log(ex.Message, LogManager.Level.ERROR);
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
                Console.WriteLine("onlineVer: " + onlineVer);
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Gets the current Windows version and sets important value 'osID'.
        /// </summary>
        /// <seealso cref="gpuInfo"> Used here, decides OS and OS architecture.</seealso>
        private static void checkWinVer()
        {
            string verOrg = Environment.OSVersion.Version.ToString();
            Boolean is64 = Environment.Is64BitOperatingSystem;

            // Windows 10
            /// After doing some research, it does not matter if you input 10 or 10+AU onto the NVIDIA website (for now - 2016-08-30).
            /// It is only (like choosing GPU model - which I WONT fix) only used for statistics, there is NO seperate
            /// drivers depending on what build you're running. But TinyNvidiaUpdateChecker wasn't made to
            /// give NVIDIA the finger, so we'll at least return the correct OS.

            /// 2016-09-x UPDATE - IMPORTANT:
            /// it seems like NVIDIA changed their website to what it used to be. What the hell?
            /// The "Windows 10 AU" has been removed from the website. If you use the Win 10AU id you will not get the latest version!
            /// I thought it has something to do with the WebClient class but apparently not.

            // Windows 10 known "version" list:
            // 1607: anniversary update
            // 1511: november update
            // 1507: original release

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
                LogManager.log(message, LogManager.Level.ERROR);
                if (showUI == true) Console.ReadKey();
                Environment.Exit(1);
            }

            if (debug == true) {
                Console.WriteLine("winVer: " + winVer);
                Console.WriteLine("osID: " + osID.ToString());
                Console.WriteLine("verOrg: " + verOrg);
                Console.WriteLine();
            }

        }

        /// <summary>
        /// Handles all the supported command line arguments </summary>
        /// <param name="theArgs"> Command line arguments in. Turned out that Environment.GetCommandLineArgs() wasn't any good.</param>
        private static void CheckArgs(string[] theArgs)
        {
            /// The command line argument handler does its work here,
            /// for a list of available arguments, use the '--help' argument.

            foreach (var arg in theArgs)
            {

                // no window
                if (arg == "--quiet") {
                    FreeConsole();
                    showUI = false;
                }

                // erase config
                else if (arg == "--erase-config") {
                    if (File.Exists(fullConfig)) {
                        try {
                            File.Delete(fullConfig);
                        } catch (Exception ex) {
                            RunIntro();
                            Console.WriteLine(ex.StackTrace);
                            Console.WriteLine();
                        }
                    }
                }

                // enable debugging
                else if (arg == "--debug") {
                    debug = true;
                }

                // force driver download
                else if (arg == "--force-dl") {
                    forceDL = true;
                }

                // show version number
                else if (arg == "--version")
                {
                    RunIntro();
                    Console.WriteLine("Current version is " + offlineVer);
                    Console.WriteLine();
                }

                // help menu
                else if (arg == "--help") {
                    RunIntro();
                    Console.WriteLine("Usage: " + Path.GetFileName(Assembly.GetEntryAssembly().Location) + " [ARGS]");
                    Console.WriteLine();
                    Console.WriteLine("--quiet        Run application quiet.");
                    Console.WriteLine("--erase-config Erase local configuration file.");
                    Console.WriteLine("--debug        Enable debugging for extended information.");
                    Console.WriteLine("--force-dl     Force download of drivers.");
                    Console.WriteLine("--version      View version number.");
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
        /// <seealso cref="gpuInfo"> Used here, decides driver download language and possibly download server.</seealso>
        private static void getLanguage()
        {
            string cultName = CultureInfo.CurrentCulture.ToString(); // https://msdn.microsoft.com/en-us/library/ee825488(v=cs.20).aspx - http://www.lingoes.net/en/translator/langcode.htm

            switch (cultName)
            {
                case "en-US":
                    langID = 1;
                    break;
                case "en-GB":
                    langID = 2;
                    break;
                case "zh-CHS":
                    langID = 5;
                    break;
                case "zh-CHT":
                    langID = 6;
                    break;
                case "ja-JP":
                    langID = 7;
                    break;
                case "ko-KR":
                    langID = 8;
                    break;
                case "de-DE":
                    langID = 9;
                    break;
                case "es-ES":
                    langID = 10;
                    break;
                case "fr-FR":
                    langID = 12;
                    break;
                case "it-IT":
                    langID = 13;
                    break;
                case "pl-PL":
                    langID = 14;
                    break;
                case "pt-BR":
                    langID = 15;
                    break;
                case "ru-RU":
                    langID = 16;
                    break;
                case "tr-TR":
                    langID = 19;
                    break;
                default:
                    // intl
                    langID = 17;
                    break;
            }

            if (debug == true) {
                Console.WriteLine("langID: " + langID);
                Console.WriteLine("cultName: " + cultName);
                Console.WriteLine();
            }
            LogManager.log("langID: " + langID, LogManager.Level.INFO);
        }

        /// <summary>
        /// A lot of things going on inside: gets current gpu driver, fetches latest gpu driver from NVIDIA server and fetches download link for latest drivers.
        /// </summary>
        private static void gpuInfo()
        {
            Console.Write("Retrieving GPU information . . . ");
            int error = 0;
            string processURL = null;
            string confirmURL = null;
            string gpuURL = null;

            // query local driver version
            try
            {
                FileVersionInfo nvidiaEXE = FileVersionInfo.GetVersionInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\System32\nvsvcr.dll"); // Sysnative? nvvsvc.exe
                OfflineGPUVersion = nvidiaEXE.FileDescription.Substring(38).Trim();
            } catch (Exception ex) {
                error++;
                OfflineGPUVersion = "000.00";
                Console.Write("ERROR!");
                LogManager.log(ex.ToString(), LogManager.Level.ERROR);
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
            }

            /// In order to proceed, we must input what GPU we have.
            /// Looking at the supported products on NVIDIA website for desktop and mobile GeForce series,
            /// we can see that they're sharing drivers with other GPU families, the only thing we have to do is tell the website
            /// if we're running a mobile or desktop GPU.
            
            int psID = 0;
            int pfID = 0;

            // loop until value is selected by user
            string key = "GPU Type";

            while (psID == 0 & pfID == 0) {
                string val = SettingManager.readSetting(key); // refresh value each time

                /// Get correct gpu drivers:
                /// you do not have to choose the exact GPU,
                /// looking at supported products, we see that the same driver package includes
                /// drivers for the majority GPU family.
                if (val == "desktop") {
                    psID = 98;  // GeForce 900-series
                    pfID = 756; // GTX 970
                } else if (val == "mobile") {
                    psID = 99;  // GeForce 900M-series (M for Mobile)
                    pfID = 758; // GTX 970M
                } else {
                    // invalid value
                    SettingManager.setupSetting(key);
                }
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
                var dates = tdReleaseDate.InnerHtml.Trim().Replace(".", string.Empty);

                var year = Convert.ToInt32(dates.Substring(0, 4));
                var month = Convert.ToInt32(dates.Substring(4, 2));
                var day = Convert.ToInt32(dates.Substring(6));

                releaseDate = new DateTime(year, month, day);
                
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

                // get download link
                htmlDocument = htmlWeb.Load(confirmURL);
                links = htmlDocument.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href"));
                foreach (var link in links) {
                    if (link.Attributes["href"].Value.Contains("download.nvidia")) {
                        downloadURL = link.Attributes["href"].Value.Trim();
                    }
                }

            } catch (Exception ex) {
                OnlineGPUVersion = "000.00";
                LogManager.log(ex.Message, LogManager.Level.ERROR);
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
                Console.WriteLine("psID: " + psID);
                Console.WriteLine("pfID: " + pfID);
                Console.WriteLine("processURL: " + processURL);
                Console.WriteLine("confirmURL: " + confirmURL);
                Console.WriteLine("gpuURL: " + gpuURL);
                Console.WriteLine("downloadURL: " + downloadURL);
                Console.WriteLine("pdfURL: " + pdfURL);
                Console.WriteLine("releaseDate: " + releaseDate.ToShortDateString());

                Console.WriteLine("OfflineGPUVersion: " + OfflineGPUVersion);
                Console.WriteLine("OnlineGPUVersion:  " + OnlineGPUVersion);
            }

        }

        /// <summary>
        /// Nothing important, just a check if the required dll is placed correctly.
        /// </summary>
        private static void checkDll()
        {
            if (!File.Exists("HtmlAgilityPack.dll")) {
                string message = "The required binary cannot be found and the application will determinate itself. It must be put in the same folder as this executable.";

                Console.WriteLine(message);
                LogManager.log(message, LogManager.Level.ERROR);
                if (showUI == true) Console.ReadKey();
                Environment.Exit(2);
            }
        }

        private static void downloadDriver()
        {
            int DateDiff = (DateTime.Now - releaseDate).Days; // how many days between the two dates
            string message = null;

            if (DateDiff == 1) {
                message = DateDiff + " day ago";
            } else if (DateDiff < 1) {
                message = "today";
            } else {
                message = DateDiff + " days ago";
            }
            
            DialogResult dialog = MessageBox.Show("There is new gpu drivers up for download, do you want to download them now?" + Environment.NewLine + Environment.NewLine + "Driver version: " + OnlineGPUVersion + Environment.NewLine + "Driver released: " + message + " (" + releaseDate.ToShortDateString() + ")", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialog == DialogResult.Yes)
            {

                Console.WriteLine();

                // @todo error handling could be better:
                // isolate saveFileDialog errors with accually downloading GPU driver

                // @todo add status bar for download progress
                // @todo do the saveFileDialog in a loop

                bool error = false;
                try
                {
                    string driverName = downloadURL.Split('/').Last();

                    // set attributes
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "Executable|*.exe";
                    saveFileDialog.Title = "Choose save file for GPU driver";
                    saveFileDialog.FileName = driverName;

                    DialogResult result = saveFileDialog.ShowDialog(); // show dialog and get status (will wait for input)

                    switch (result)
                    {
                        case DialogResult.OK:
                            savePath = saveFileDialog.FileName.ToString();
                            break;

                        default:
                            // savePath = Path.GetTempPath() + driverName;

                            // if something went wrong, fall back to downloads folder
                            savePath = getDownloadFolderPath() + driverName;
                            break;
                    }

                    if (debug == true)
                    {
                        Console.WriteLine("savePath: " + savePath);
                        Console.WriteLine("result: " + result);
                    }
                    
                    Console.Write("Downloading the driver . . . ");
                    
                    using (WebClient webClient = new WebClient())
                    {
                        var notifier = new AutoResetEvent(false);
                        var progress = new ProgressBar();

                        webClient.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e)
                        {
                            progress.Report((double)e.ProgressPercentage / 100);

                            if (e.BytesReceived >= e.TotalBytesToReceive) notifier.Set();
                        };

                        webClient.DownloadFileAsync(new Uri(downloadURL), savePath);

                        notifier.WaitOne(); // sync with the above
                        progress.Dispose(); // get rid of the progress bar
                    }

                }
                catch (Exception ex)
                {
                    error = true;
                    Console.Write("ERROR!");
                    LogManager.log(ex.Message, LogManager.Level.ERROR);
                    Console.WriteLine();
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine();
                }

                if (error == false)
                {
                    Console.Write("OK!");
                    Console.WriteLine();
                }

                
                Console.WriteLine();
                Console.WriteLine("The downloaded file has been saved at: " + savePath);

                // craft pdf link, last resort
                if (pdfURL == null) {
                    if(downloadURL.Contains("desktop")) {
                        pdfURL = "http://us.download.nvidia.com/Windows/" + OnlineGPUVersion + "/" + OnlineGPUVersion + "-win10-win8-win7-desktop-release-notes.pdf";
                    } else {
                        pdfURL = "http://us.download.nvidia.com/Windows/" + OnlineGPUVersion + "/" + OnlineGPUVersion + "-win10-win8-win7-notebook-release-notes.pdf";
                    }
                    Console.WriteLine("No release notes found, but a link to the notes has been crafted by following the template Nvidia uses.");
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

                dialog = MessageBox.Show("Do you wish to run the driver installer?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialog == DialogResult.Yes) {
                    try {
                        Process.Start(savePath);
                    } catch (Exception ex) {
                        Console.WriteLine(ex.StackTrace);
                    }
                    
                }
            }
        }

        private static string getDownloadFolderPath()
        {
            string downloadPath = null;
            SHGetKnownFolderPath(new Guid("374DE290-123F-4565-9164-39C4925E467B"), 0, IntPtr.Zero, out downloadPath);

            return downloadPath + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Intro with legal message for cleanup at the top.
        /// </summary>
        private static void RunIntro()
        {
            if(!HasIntro) {
                HasIntro = true;
                Console.WriteLine("TinyNvidiaUpdateChecker v" + offlineVer + " dev build");
                //Console.WriteLine("TinyNvidiaUpdateChecker v" + offlineVer);
                Console.WriteLine();
                Console.WriteLine("Copyright (C) 2016 Hawaii_Beach");
                Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY");
                Console.WriteLine("This is free software, and you are welcome to redistribute it");
                Console.WriteLine("under certain conditions. Licensed under GPLv3.");
                Console.WriteLine();
            } else {
                LogManager.log("Intro has already been run!", LogManager.Level.INFO);
            }
        }

    }
}