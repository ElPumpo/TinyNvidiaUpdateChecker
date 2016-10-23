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
        private static int onlineVer;

        /// <summary>
        /// Current GPU driver version
        /// </summary>
        private static int offlineGPUDriverVersion;

        /// <summary>
        /// Remote GPU driver version
        /// </summary>
        private static int onlineGPUDriverVersion;

        /// <summary>
        /// Langauge ID for GPU driver download
        /// </summary>
        private static int langID;

        private static string downloadURL;
        private static string savePath;
        private static string pdfURL;

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

            CheckArgs();

            introMessage();

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

            if (onlineGPUDriverVersion == offlineGPUDriverVersion) {
                Console.WriteLine("Your GPU drivers are up-to-date!");
            } else {
                if (offlineGPUDriverVersion > onlineGPUDriverVersion) {
                    Console.WriteLine("Your current GPU driver is newer than remote!");}
                if (onlineGPUDriverVersion < offlineGPUDriverVersion) {
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
        public static void configInit()
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
                Console.WriteLine("The configuration file is located at: " + dirToConfig);

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
                HtmlWeb webClient = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument htmlDocument = webClient.Load(serverURL);

                // get version
                HtmlNode tdVer = htmlDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == "currentVersion");
                onlineVer = Convert.ToInt32(tdVer.InnerText.Replace(".", string.Empty));

            } catch (Exception ex) {
                error++;
                Console.Write("ERROR!");
                LogManager.log(ex.Message, LogManager.Level.ERROR);
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
            }
            if (error == 0) {
                Console.Write("OK!");
                Console.WriteLine();
            }
            int iOfflineVer = Convert.ToInt32(offlineVer.Replace(".", string.Empty));

            if (onlineVer > iOfflineVer) {
                Console.WriteLine("There is a update available for TinyNvidiaUpdateChecker!");
                DialogResult dialog = MessageBox.Show("There is a new client update available to download, do you want to be navigate to the official GitHub download section?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialog == DialogResult.Yes) {
                    Process.Start("https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases");
                }
            }

            if (debug == true)
            {
                Console.WriteLine("iOfflineVer: " + iOfflineVer);
                Console.WriteLine("onlineVer:   " + onlineVer);
            }
            Console.WriteLine();
        } // checks for application updates

        /// <summary>
        /// Gets the current Windows version and sets important value 'osID'.
        /// </summary>
        /// <seealso cref="gpuInfo"> Used here, decides OS and OS architecture.</seealso>
        private static void checkWinVer()
        {
            string verOrg = Environment.OSVersion.Version.ToString();
            Boolean is64 = Environment.Is64BitOperatingSystem;

            // Windows 10 + AU
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

            /*
            if (verOrg.Contains("10.0")) {
                int release = 0;

                try {

                    // different keys for different OS installs
                    string subKey = null;
                    subKey = is64
                        ? @"SOFTWARE\WOW6432Node\Microsoft\Windows NT\CurrentVersion"
                        : @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

                    RegistryKey key = Registry.LocalMachine.OpenSubKey(subKey);
                    release = Convert.ToInt32(key.GetValue("ReleaseId")); // convert the "version" to a int
                    key.Close();
                } catch (Exception ex) {
                    Console.WriteLine("ERROR!");
                    LogManager.log(ex.Message, LogManager.Level.ERROR);
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine();
                }
                */

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
        /// Handles all the supported command line arguments
        /// </summary>
        private static void CheckArgs()
        {
            /// The command line argument handler does its work here,
            /// for a list of available arguments, use the '--help' argument.

            foreach (var arg in Environment.GetCommandLineArgs().Skip(1))
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
                            Console.WriteLine(ex.GetType().Name + " - Could not erase the config!");
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
                    Console.WriteLine("Current version is " + offlineVer);
                    Console.WriteLine();
                }

                // help menu
                else if (arg == "--help") {
                    introMessage();
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
                    Console.WriteLine("Unknown command '" + arg + "', type --help for help.");
                    Console.WriteLine();
                }
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
            Console.Write("Looking up GPU information . . . ");
            int error = 0;
            string processURL = null;
            string confirmURL = null;
            string gpuURL = null;

            // query local driver version
            try
            {
                FileVersionInfo nvidiaEXE = FileVersionInfo.GetVersionInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\System32\nvsvcr.dll"); // Sysnative? nvvsvc.exe
                offlineGPUDriverVersion = Convert.ToInt32(nvidiaEXE.FileDescription.Substring(38).Trim().Replace(".", string.Empty));
            } catch (FileNotFoundException ex) {
                error++;
                Console.Write("ERROR!");
                LogManager.log(ex.Message, LogManager.Level.ERROR);
                Console.WriteLine();
                Console.WriteLine("The required executable is not there! Are you sure you've at least installed NVIDIA GPU drivers once?");

            } catch (Exception ex) {
                error++;
                Console.Write("ERROR!");
                LogManager.log(ex.Message, LogManager.Level.ERROR);
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

            while (psID == 0 && pfID == 0) {
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
            try
            {
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
                onlineGPUDriverVersion = Convert.ToInt32(tdVer.InnerHtml.Trim().Substring(0, 6).Replace(".", string.Empty));

                
                IEnumerable<HtmlNode> links = htmlDocument.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href"));
                foreach (var link in links) {

                    // get driver URL
                    if (link.Attributes["href"].Value.Contains("/content/DriverDownload-March2009/")) {
                        confirmURL = "http://www.nvidia.com" + link.Attributes["href"].Value;
                    }

                    // get release notes URL
                    if (link.Attributes["href"].Value.Contains("release-notes.pdf")) {
                        pdfURL = link.Attributes["href"].Value;
                    }
                }

                // get download link
                htmlDocument = htmlWeb.Load(confirmURL);
                links = htmlDocument.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href"));
                foreach (var link in links) {
                    if (link.Attributes["href"].Value.Contains("download.nvidia")) {
                        downloadURL = link.Attributes["href"].Value;
                    }
                }

            } catch (Exception ex) {
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

                Console.WriteLine("offlineGPUDriverVersion: " + offlineGPUDriverVersion);
                Console.WriteLine("onlineGPUDriverVersion:  " + onlineGPUDriverVersion);
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

        /// <summary>
        /// Intro with legal message for cleanup at the top.
        /// </summary>
        private static void introMessage()
        {
            Console.WriteLine("TinyNvidiaUpdateChecker v" + offlineVer + " dev build");
            //Console.WriteLine("TinyNvidiaUpdateChecker v" + offlineVer);
            Console.WriteLine();
            Console.WriteLine("Copyright (C) 2016 Hawaii_Beach");
            Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY");
            Console.WriteLine("This is free software, and you are welcome to redistribute it");
            Console.WriteLine("under certain conditions. Licensed under GPLv3.");
            Console.WriteLine();
        }

        private static void downloadDriver()
        {
            DialogResult dialog = MessageBox.Show("There is a new update available to download, do you want to download the update?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();
                }

                if (error == false)
                {
                    Console.Write("OK!");
                    Console.WriteLine();
                }

                
                Console.WriteLine();
                Console.WriteLine("The downloaded file has been saved at: " + savePath);

                dialog = MessageBox.Show("Do you want view the release PDF?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialog == DialogResult.Yes) {
                    Process.Start(pdfURL);
                }

                dialog = MessageBox.Show("Do you wish to run the driver installer?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialog == DialogResult.Yes) {
                    try {
                        Process.Start(savePath);
                    } catch (Exception ex) {
                        Console.WriteLine(ex.Message);
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

    }
}
