using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker {

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

    class mainConsole {

        // Client updater stuff
        private readonly static string serverURL = "https://raw.githubusercontent.com/ElPumpo/TinyNvidiaUpdateChecker/master/TinyNvidiaUpdateChecker/version";
        private static int offlineVer = 0200;
        private static string sOnlineVer;
        private static int onlineVer;

        // GPU driver version related stuff
        private static string offlineGPUDriverVersion;
        private static string onlineGPUDriverVersion;

        private static int iOfflineGPUDriverVersion;
        private static int iOnlineGPUDriverVersion;


        private static string winVer;

        private static string dirToConfig = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Hawaii_Beach\TinyNvidiaUpdateChecker\";

        static iniFile ini = new iniFile(dirToConfig + "config.ini");

        static int showC = 1;


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
                    showC = 0;
                }
            }

            if (showC == 1)
            {
                AllocConsole();
            }
            Console.Title = "TinyNvidiaUpdateChecker v" + offlineVer + "PRE-ALPHA";
            Console.WriteLine("TinyNvidiaUpdateChecker v" + offlineVer + "PRE-ALPHA");
            Console.WriteLine();
            Console.WriteLine("Copyright (C) 2016 Hawaii_Beach");
            Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY");
            Console.WriteLine("This is free software, and you are welcome to redistribute it");
            Console.WriteLine("under certain conditions. Licensed under GPLv3.");
            Console.WriteLine();

            iniInit(); // read & write configuration file

            if(ini.IniReadValue("Configuration", "Check for Updates") == "1" )
            {
                checkForUpdates();
            }
            
            
            checkWinVer();

            checkOnlineVersion();
            checkOfflineVersion();

            Console.WriteLine("offlineGPUDriverVersion: " + offlineGPUDriverVersion);
            Console.WriteLine("onlineGPUDriverVersion:  " + onlineGPUDriverVersion);

            try
            {
                iOfflineGPUDriverVersion = Convert.ToInt32(offlineGPUDriverVersion.Replace(".", string.Empty));
                iOnlineGPUDriverVersion = Convert.ToInt32(onlineGPUDriverVersion.Replace(".", string.Empty));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }



            if (iOnlineGPUDriverVersion == iOfflineGPUDriverVersion)
                {
                    Console.WriteLine("GPU drivers are up-to-date!");
                }
                else
                {
                    if (iOfflineGPUDriverVersion > iOnlineGPUDriverVersion)
                    {
                        Console.WriteLine("Current GPU driver is newer than remote!");
                    }

                    if (iOnlineGPUDriverVersion < iOfflineGPUDriverVersion)
                    {
                        Console.WriteLine("GPU drivers are up-to-date!");

                    }
                    else
                    {
                        Console.WriteLine("Newer GPU drivers are available!");
                    }
                }
            

            //Console.WriteLine("iOfflineGPUDriverVersion: " + iOfflineGPUDriverVersion);
            //Console.WriteLine("iOnlineGPUDriverVersion:  " + iOnlineGPUDriverVersion);


            Console.WriteLine();
            Console.WriteLine("Job done! Press any key to exit.");
            if(showC == 1)
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
                ini.IniWriteValue("Misc", "Force onlineGPUDriverVersion", "260.00");
            }

            if (!File.Exists(dirToConfig + "config.ini"))
            {
                Console.WriteLine("Generating configuration file, folder already exists.");
                ini.IniWriteValue("Configuration", "Check for Updates", "1");
                ini.IniWriteValue("Misc", "Force onlineGPUDriverVersion", "260.00");
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
            }
            //Windows 8.1
            else if (verOrg.Contains("6.3"))
            {
                winVer = "8.1";
            }
            //Windows 8
            else if (verOrg.Contains("6.2"))
            {
                winVer = "8";
            }
            //Windows 7
            else if (verOrg.Contains("6.1"))
            {
                winVer = "7";
            }
            //Windows Vista
            else if (verOrg.Contains("6.0"))
            {
                winVer = "Vista";
            }
            else {
                winVer = "Unknown";
                Console.WriteLine("You're running a non-supported version of Windows; the application will determine itself.");
                Console.WriteLine("OS: " + verOrg);
                if (showC == 1)
                {
                    Console.ReadKey();
                }
                Environment.Exit(1);
            }

            Console.WriteLine("winVer: " + winVer);
            Console.WriteLine();

        } // gets local Windows version

        private static void checkOnlineVersion()
        {
            Console.WriteLine("CheckOnlineVersion is under construction!");
            //onlineGPUDriverVersion = "220.21"; // debug
            onlineGPUDriverVersion = ini.IniReadValue("Misc", "Force onlineGPUDriverVersion");

            if (onlineGPUDriverVersion.Length != 6)
            {
                Console.WriteLine("Invalid length of onlineGPUDriverVersion!");
                Console.WriteLine("It's currently used to emulate a remote version from NVIDIA servers.");
                Console.WriteLine("Current onlineGPUDriverVersion: " + onlineGPUDriverVersion);
                Console.WriteLine("Template: XXX.XX");
            }


        } // (todo) fetch latest NVIDIA GPU driver version

        private static void checkOfflineVersion()
        {
            try
            {
                FileVersionInfo nvvsvcExe = FileVersionInfo.GetVersionInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\System32\nvvsvc.exe"); // Sysnative?
                offlineGPUDriverVersion = nvvsvcExe.FileDescription.Substring(38).Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Could not find the required NVIDIA executable.");
                if (showC == 1)
                {
                    Console.ReadKey();
                }
                Environment.Exit(1);
            }
        } // gets current NVIDIA GPU driver version
    }
}
