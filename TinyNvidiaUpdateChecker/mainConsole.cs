using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace TinyNvidiaUpdateChecker
{

    /*
    TinyNvidiaUpdateChecker - Check for NVIDIA desktop GPU drivers, GeForce Experience replacer
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

    class mainConsole
    {

        /// <summary>
        /// Server adress
        /// </summary>
        private readonly static string serverURL = "https://raw.githubusercontent.com/ElPumpo/TinyNvidiaUpdateChecker/master/TinyNvidiaUpdateChecker/version";

        /// <summary>
        /// Current client version
        /// </summary>
        private static int offlineVer = 1100;

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
        private static int language;

        /// <summary>
        /// OS ID for GPU driver download
        /// </summary>
        private static string osID;

        private static string finalURL;
        private static string driverURL;

        /// <summary>
        /// Local Windows version (not used?)
        /// </summary>
        private static string winVer;

        /// <summary>
        /// Direction for configuration folder
        /// </summary>
        private static string dirToConfig = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Hawaii_Beach\TinyNvidiaUpdateChecker\";

        /// <summary>
        /// Direction for configuration file
        /// </summary>
        private static iniFile ini = new iniFile(dirToConfig + "config.ini");

        /// <summary>
        /// Show UI or go quiet | 1: show | 0: quiet
        /// </summary>
        private static int showUI = 1;

        /// <summary>
        /// Enable extended information
        /// </summary>
        private static int debug = 0;


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [STAThread]
        static void Main(string[] args)
        {
            string[] parms = Environment.GetCommandLineArgs();
            if (parms.Length > 1)
            {
                // go quiet mode
                if(Array.IndexOf(parms, "--quiet") != -1)
                {
                    FreeConsole();
                    showUI = 0;
                }

                if(Array.IndexOf(parms, "--debug") != -1) debug = 1; // enable debug

                if (Array.IndexOf(parms, "--?") != -1) // help menu
                {
                    Console.WriteLine("TinyNvidiaUpdateChecker v" + offlineVer);
                    Console.WriteLine();
                    Console.WriteLine("Copyright (C) 2016 Hawaii_Beach");
                    Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY");
                    Console.WriteLine("This is free software, and you are welcome to redistribute it");
                    Console.WriteLine("under certain conditions. Licensed under GPLv3.");
                    Console.WriteLine();
                    Console.WriteLine("Usage: " + Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location) + " [--quiet] [--debug] [--?]");
                    Console.WriteLine();
                    Console.WriteLine("--quiet        Application runs quiet.");
                    Console.WriteLine("--debug        Enable debugging for extended information.");
                    Console.WriteLine("--?            Displays this message.");
                    Console.WriteLine();
                    Environment.Exit(0);
                }
            }
            if (showUI == 1) AllocConsole();

            Console.Title = "TinyNvidiaUpdateChecker v" + offlineVer;
            Console.WriteLine("TinyNvidiaUpdateChecker v" + offlineVer);
            Console.WriteLine();
            Console.WriteLine("Copyright (C) 2016 Hawaii_Beach");
            Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY");
            Console.WriteLine("This is free software, and you are welcome to redistribute it");
            Console.WriteLine("under certain conditions. Licensed under GPLv3.");
            Console.WriteLine();

            checkDll();

            iniInit(); // read & write configuration file

            checkWinVer(); // get current windows version

            getLanguage(); // get current langauge

            if(ini.IniReadValue("Configuration", "Check for Updates") == "1") searchForUpdates();

            gpuInfo();

            if(onlineGPUDriverVersion == offlineGPUDriverVersion)
            {
                Console.WriteLine("GPU drivers are up-to-date!");
            }
            else
            {
                if(offlineGPUDriverVersion > onlineGPUDriverVersion)
                {
                    Console.WriteLine("Current GPU driver is newer than remote!");
                }

                if(onlineGPUDriverVersion < offlineGPUDriverVersion)
                {
                    Console.WriteLine("GPU drivers are up-to-date!");
                }
                else
                {
                    Console.WriteLine("There are new drivers to download!");
                    DialogResult dialog = MessageBox.Show("There's a new update available to download, do you want to download the update now?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if(dialog == DialogResult.Yes)
                    {
                        Process.Start(driverURL);
                    }
                        
                }
            }

            if(debug == 1)
            {
                Console.WriteLine("offlineGPUDriverVersion: " + offlineGPUDriverVersion);
                Console.WriteLine("onlineGPUDriverVersion:  " + onlineGPUDriverVersion);
            }

            Console.WriteLine();
            Console.WriteLine("Job done! Press any key to exit.");
            if(showUI == 1) Console.ReadKey();
            Environment.Exit(0);
        }

        private static void iniInit()
        {
            // create dir if it doesn't exist
            if(!Directory.Exists(dirToConfig)) Directory.CreateDirectory(dirToConfig);

            // create config file
            if(!File.Exists(dirToConfig + "config.ini"))
            {
                Console.WriteLine("Generating configuration file, this only happenes once.");
                Console.WriteLine("The configuration file is located at " + dirToConfig);
                ini.IniWriteValue("Configuration", "Check for Updates", "1");
                Console.WriteLine();
            }

        } // configuration files

        private static void searchForUpdates()
        {
            Console.Write("Searching for Updates . . . ");
            int error = 0;
            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(serverURL);
                StreamReader reader = new StreamReader(stream);
                onlineVer = Convert.ToInt32(reader.ReadToEnd());
                reader.Close();
                stream.Close();
            }

            catch (Exception ex)
            {
                error = 1;
                Console.Write("ERROR!");
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
            }
            if(error == 0)
            {
                Console.Write("OK!");
                Console.WriteLine();
            }

            if(onlineVer > offlineVer)
            {
                Console.WriteLine("There is a update available for TinyNvidiaUpdateChecker!");
                DialogResult dialog = MessageBox.Show("There's a new client update available to download, do you want to be navigate to the page?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if(dialog == DialogResult.Yes)
                {
                    Process.Start("https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases");
                }
            }

            if(debug == 1)
            {
                Console.WriteLine("offlineVer: " + offlineVer);
                Console.WriteLine("onlineVer:  " + onlineVer);
            }
            Console.WriteLine();
        } // checks for application updates

        private static void checkWinVer()
        {
            string verOrg = Environment.OSVersion.Version.ToString();

            //Windows 10
            if (verOrg.Contains("10.0"))
            {
                winVer = "10";
                if(Environment.Is64BitOperatingSystem == true)
                {
                    osID = "57";
                } else {
                    osID = "56";
                }
            }
            //Windows 8.1
            else if (verOrg.Contains("6.3"))
            {
                winVer = "8.1";
                if (Environment.Is64BitOperatingSystem == true)
                {
                    osID = "41";
                } else {
                    osID = "40";
                }
            }
            //Windows 8
            else if (verOrg.Contains("6.2"))
            {
                winVer = "8";
                if (Environment.Is64BitOperatingSystem == true)
                {
                    osID = "41";
                } else {
                    osID = "40";
                }
            }
            //Windows 7
            else if (verOrg.Contains("6.1"))
            {
                winVer = "7";
                if (Environment.Is64BitOperatingSystem == true)
                {
                    osID = "41";
                } else {
                    osID = "40";
                }
            }
            //Windows Vista
            else if (verOrg.Contains("6.0"))
            {
                winVer = "Vista";
                if (Environment.Is64BitOperatingSystem == true)
                {
                    osID = "41";
                } else {
                    osID = "40";
                }
            } else {
                winVer = "Unknown";
                Console.WriteLine("You're running a non-supported version of Windows; the application will determine itself.");
                Console.WriteLine("OS: " + verOrg);
                if(showUI == 1) Console.ReadKey();
                Environment.Exit(1);
            }

            if(debug == 1)
            {
                Console.WriteLine("winVer: " + winVer);
                Console.WriteLine("osID: " + osID);
                Console.WriteLine();
            }
            
            

        } // get local Windows version

        private static void getLanguage()
        {
            string cultName = CultureInfo.CurrentCulture.ToString(); // https://msdn.microsoft.com/en-us/library/ee825488(v=cs.20).aspx

            switch(cultName)
            {
                case "en-US":
                    language = 1;
                    break;
                case "en-GB":
                    language = 2;
                    break;
                case "zh-CHS":
                    language = 5;
                    break;
                case "zh-CHT":
                    language = 6;
                    break;
                case "ja-JP":
                    language = 7;
                    break;
                case "ko-KR":
                    language = 8;
                    break;
                case "de-DE":
                    language = 9;
                    break;
                case "es-ES":
                    language = 10;
                    break;
                case "fr-FR":
                    language = 12;
                    break;
                case "it-IT":
                    language = 13;
                    break;
                case "pl-PL":
                    language = 14;
                    break;
                case "pt-BR":
                    language = 15;
                    break;
                case "ru-RU":
                    language = 16;
                    break;
                case "tr-TR":
                    language = 19;
                    break;
                default:
                    language = 17;
                    break;
            }

            if(debug == 1)
            {
                Console.WriteLine("cultName: " + cultName);
                Console.WriteLine("language: " + language);
                Console.WriteLine();
            }
        } // decide driver langauge

        private static void gpuInfo()
        {
            Console.Write("Looking up GPU information . . . ");
            int error = 0;

            // query local driver version
            try
            {
                FileVersionInfo nvvsvcExe = FileVersionInfo.GetVersionInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\System32\nvvsvc.exe"); // Sysnative?
                offlineGPUDriverVersion = Convert.ToInt32(nvvsvcExe.FileDescription.Substring(38).Trim().Replace(".", string.Empty));
            }
            catch (Exception ex)
            {
                error = 1;
                Console.Write("ERROR!");
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
            }

            // get remote version
            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead("http://www.nvidia.com/Download/processDriver.aspx?psid=98&pfid=756&rpf=1&osid=" + osID + "&lid=" + language.ToString() + "&ctk=0");
                StreamReader reader = new StreamReader(stream);
                finalURL = reader.ReadToEnd();
                reader.Close();
                stream.Close();
            }
            catch (Exception ex)
            {
                if(error == 0)
                {
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    error = 1;
                }
                Console.WriteLine(ex.StackTrace);
            }

            try
            {
                // HTMLAgilityPack
                // thanks to http://www.codeproject.com/Articles/691119/Html-Agility-Pack-Massive-information-extraction-f for a great article

                HtmlWeb webClient = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument htmlDocument = webClient.Load(finalURL);

                // get version
                HtmlNode tdVer = htmlDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == "tdVersion");
                onlineGPUDriverVersion = Convert.ToInt32(tdVer.InnerHtml.Trim().Substring(0, 6).Replace(".", string.Empty));

                // get driver URL
                IEnumerable<HtmlNode> links = htmlDocument.DocumentNode.Descendants("a").Where(x => x.Attributes.Contains("href"));
                foreach (var link in links)
                {
                    if(link.Attributes["href"].Value.Contains("/content/DriverDownload-March2009/"))
                    {
                        driverURL = "http://www.nvidia.com" + link.Attributes["href"].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                if (error == 0)
                {
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    error = 1;
                }
                Console.WriteLine(ex.StackTrace);
            }
            if(error == 0)
            {
                Console.Write("OK!");
                Console.WriteLine();
            }

        } // get local and remote GPU driver version

        private static void checkDll()
        {
            if(!File.Exists("HtmlAgilityPack.dll"))
            {
                Console.WriteLine("The required binary cannot be found and the application will determinate itself. It must be put in the same folder as this executable.");
                if(showUI == 1) Console.ReadKey();
                Environment.Exit(2);
            }
        }
    }
}