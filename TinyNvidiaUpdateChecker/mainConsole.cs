using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace TinyNvidiaUpdateChecker
{

    /*
    TinyNvidiaUpdateChecker - tiny application which checks for GPU drivers daily.
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

        // client updater stuff
        private readonly static string serverURL = "https://raw.githubusercontent.com/ElPumpo/TinyNvidiaUpdateChecker/master/TinyNvidiaUpdateChecker/version";
        private static int offlineVer = 1000;
        private static string sOnlineVer;
        private static int onlineVer;

        private static int offlineGPUDriverVersion;
        private static int onlineGPUDriverVersion;

        // other

        // nvidia http request stuff
        private static string language;
        private static string osID;
        private static string finalURL;
        private static string driverURL;

        private static string winVer;

        private static string dirToConfig = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Hawaii_Beach\TinyNvidiaUpdateChecker\";

        static iniFile ini = new iniFile(dirToConfig + "config.ini");

        static int showUI = 1;


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
                if (Array.IndexOf(parms, "-quiet") != -1)
                {
                    FreeConsole();
                    showUI = 0;
                }
            }

            if (showUI == 1)
            {
                AllocConsole();
            }
            Console.Title = "TinyNvidiaUpdateChecker v" + offlineVer;
            Console.WriteLine("TinyNvidiaUpdateChecker v" + offlineVer);
            Console.WriteLine();
            Console.WriteLine("Copyright (C) 2016 Hawaii_Beach");
            Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY");
            Console.WriteLine("This is free software, and you are welcome to redistribute it");
            Console.WriteLine("under certain conditions. Licensed under GPLv3.");
            Console.WriteLine();

            iniInit(); // read & write configuration file

            if (ini.IniReadValue("Configuration", "Check for Updates") == "1")
            {
                checkForUpdates();
            }

            checkWinVer();

            checkOfflineVersion();
            checkOnlineVersion();

            Console.WriteLine("offlineGPUDriverVersion: " + offlineGPUDriverVersion);
            Console.WriteLine("onlineGPUDriverVersion:  " + onlineGPUDriverVersion);

            if (onlineGPUDriverVersion == offlineGPUDriverVersion)
            {
                Console.WriteLine("GPU drivers are up-to-date!");
            }
            else
            {
                if (offlineGPUDriverVersion > onlineGPUDriverVersion)
                {
                    Console.WriteLine("Current GPU driver is newer than remote!");
                }

                if (onlineGPUDriverVersion < offlineGPUDriverVersion)
                {
                    Console.WriteLine("GPU drivers are up-to-date!");

                }
                else
                {
                    DialogResult dialog = MessageBox.Show("There's a new update available to download, do you want to download the update now?", "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (dialog == DialogResult.Yes)
                    {
                        Process.Start(driverURL);
                    }
                        
                }
            }

            Console.WriteLine();
            Console.WriteLine("Job done! Press any key to exit.");
            if (showUI == 1)
            {
                Console.ReadKey();
            }
            Application.Exit();
        }

        private static void iniInit()
        {
            if (!Directory.Exists(dirToConfig))
            {
                Console.WriteLine("Generating configuration file, this only happenes once.");
                Console.WriteLine("The configuration file is located at: " + dirToConfig);
                Directory.CreateDirectory(dirToConfig);
                ini.IniWriteValue("Configuration", "Check for Updates", "1");
            }

            if (!File.Exists(dirToConfig + "config.ini"))
            {
                Console.WriteLine("Generating configuration file, folder already exists.");
                ini.IniWriteValue("Configuration", "Check for Updates", "1");
            }

        } // configuration files

        private static void checkForUpdates()
        {
            Console.WriteLine("-----UPDATE CHECKER-----");
            Console.WriteLine("Checking for Updates . . .");
            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(serverURL);
                stream.ReadTimeout = 5;
                StreamReader reader = new StreamReader(stream);
                sOnlineVer = reader.ReadToEnd();
                reader.Close();
                stream.Close();

                onlineVer = Convert.ToInt32(sOnlineVer);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Could not search for updates!");
                sOnlineVer = null;
            }

            //IF start
            if (onlineVer == offlineVer)
            {
                Console.WriteLine("Client is up-to-date!");


            }
            else
            {
                if (offlineVer > onlineVer)
                {
                    Console.WriteLine("OfflineVer is greater than OnlineVer!");
                }

                if (onlineVer < offlineVer)
                {
                    Console.WriteLine("Client is up-to-date!");

                }
                else
                {
                    Console.WriteLine("Update available!");
                    Console.WriteLine("Please visit the official GitHub page and download the latest version.");
                    Process.Start("https://github.com/ElPumpo/TinyNvidiaUpdateChecker/releases");
                }

            }
            Console.WriteLine("offlineVer: " + offlineVer);
            Console.WriteLine("onlineVer:  " + onlineVer);
            Console.WriteLine("----UPDATE CHECKER END--");
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
                } else
                {
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
                }
                else
                {
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
                }
               else
                {
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
                }
                else
                {
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
                }
                else
                {
                    osID = "40";
                }
            }
            else
            {
                winVer = "Unknown";
                Console.WriteLine("You're running a non-supported version of Windows; the application will determine itself.");
                Console.WriteLine("OS: " + verOrg);
                if (showUI == 1)
                {
                    Console.ReadKey();
                }
                Environment.Exit(1);
            }

            Console.WriteLine("winVer: " + winVer);
            Console.WriteLine();

        } // get local Windows version

        private static void checkOnlineVersion()
        {

            // get url
            language = "17";
            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead("http://www.nvidia.com/Download/processDriver.aspx?psid=98&pfid=756&rpf=1&osid=" + osID + "&lid=" + language + "&ctk=0");
                stream.ReadTimeout = 5000;
                StreamReader reader = new StreamReader(stream);
                finalURL = reader.ReadToEnd();
                reader.Close();
                stream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Error processing HTML request at processDriver.aspx!");
                if (showUI == 1)
                {
                    Console.ReadKey();
                }
                Environment.Exit(1);
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
                    if (link.Attributes["href"].Value.Contains("/content/DriverDownload-March2009/"))
                    {
                        driverURL = "http://www.nvidia.com" + link.Attributes["href"].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Error processing HTML request at HTMLAgilityPack!");
                if (showUI == 1)
                {
                    Console.ReadKey();
                }
                Environment.Exit(1);
            }

        }

        private static void checkOfflineVersion()
        {

            // get driver version
            try
            {
                FileVersionInfo nvvsvcExe = FileVersionInfo.GetVersionInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\System32\nvvsvc.exe"); // Sysnative?
                offlineGPUDriverVersion = Convert.ToInt32(nvvsvcExe.FileDescription.Substring(38).Trim().Replace(".", string.Empty));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Could not find the required NVIDIA executable.");
                if (showUI == 1)
                {
                    Console.ReadKey();
                }
                Environment.Exit(1);
            }

        } // gets current NVIDIA GPU driver version

    }
}
