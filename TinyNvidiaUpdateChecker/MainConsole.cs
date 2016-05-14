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

    class MainConsole {

        // Client updater stuff
        private readonly static string serverURL = "https://raw.githubusercontent.com/ElPumpo/TinyNvidiaUpdateChecker/master/TinyNvidiaUpdateChecker/version";
        private static int OfflineVer = 0100;
        private static string sOnlineVer;
        private static int OnlineVer;
        private static bool ErrorExists = false;

        // GPU driver version related stuff
        private static string OfflineGPUDriverVersion;
        private static string OnlineGPUDriverVersion;

        private static int iOfflineGPUDriverVersion;
        private static int iOnlineGPUDriverVersion;


        private static string WinVer;

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
            Console.Title = "TinyNvidiaUpdateChecker";
            Console.WriteLine("TinyNvidiaUpdateChecker v" + OfflineVer + "DEV launching . . .");

            iniInit(); // read & write configuration file

            if(ini.IniReadValue("Configuration", "CheckForUpdates") == "1" )
            {
                checkForUpdates();
            }
            
            
            checkWinVer();

            checkOnlineVersion();
            checkOfflineVersion();

            Console.WriteLine("OfflineGPUDriverVersion: " + OfflineGPUDriverVersion);
            Console.WriteLine("OnlineGPUDriverVersion:  " + OnlineGPUDriverVersion);

            iOfflineGPUDriverVersion = Convert.ToInt32(OfflineGPUDriverVersion.Replace(".", string.Empty));
            iOnlineGPUDriverVersion = Convert.ToInt32(OnlineGPUDriverVersion.Replace(".", string.Empty));

            
                if (iOnlineGPUDriverVersion == iOfflineGPUDriverVersion)
                {
                    Console.WriteLine("GPU drivers are up-to-date");
                }
                else
                {
                    if (iOfflineGPUDriverVersion > iOnlineGPUDriverVersion)
                    {
                        Console.WriteLine("iOfflineGPUDriverVersion is greater than iOnlineGPUDriverVersion!");
                    }

                    if (iOnlineGPUDriverVersion < iOfflineGPUDriverVersion)
                    {
                        Console.WriteLine("GPU drivers are up-to-date");

                    }
                    else
                    {
                        Console.WriteLine("Newer GPU drivers available");
                    }
                }
            

            //Console.WriteLine("iOfflineGPUDriverVersion: " + iOfflineGPUDriverVersion);
            //Console.WriteLine("iOnlineGPUDriverVersion:  " + iOnlineGPUDriverVersion);


            Console.WriteLine();
            Console.WriteLine("Job done! Press any key to exit.");
            Console.ReadKey();
            Application.Exit();


        }

        private static void iniInit()
        {
            if (!Directory.Exists(dirToConfig))
            {
                Console.WriteLine("Generating configuration file, this only happenes once.");
                Console.WriteLine("The configuration file is located at: " + dirToConfig);
                Directory.CreateDirectory(dirToConfig);
                ini.IniWriteValue("Configuration", "CheckForUpdates", "1");
            }

            if (!File.Exists(dirToConfig + "config.ini"))
            {
                Console.WriteLine("Generating configuration file, folder already exists.");
                ini.IniWriteValue("Configuration", "CheckForUpdates", "1");
            }


        } // configuration files

        private static void checkForUpdates()
        {
            Console.WriteLine();
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

                OnlineVer = Convert.ToInt32(sOnlineVer);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Could not search for updates!");
                ErrorExists = true;
                sOnlineVer = null;
            }

            //IF start
            if (ErrorExists == false)
            {
                if (OnlineVer == OfflineVer)
                {
                    Console.WriteLine("Client is up-to-date!");


                }
                else
                {
                    if (OfflineVer > OnlineVer)
                    {
                        Console.WriteLine("OfflineVer is greater than OnlineVer!");
                    }

                    if (OnlineVer < OfflineVer)
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
                //IF end
            }
            Console.WriteLine("OfflineVer: " + OfflineVer);
            Console.WriteLine("OnlineVer:  " + OnlineVer);
            Console.WriteLine("----UPDATE CHECKER END--");
        } // checks for application updates

        private static void checkWinVer()
        {
            string WinVerOriginal = Environment.OSVersion.Version.ToString();

            //Windows 10
            if (WinVerOriginal.Contains("10"))
            {
                WinVer = "10";

            }
            //Windows 8.1
            else if (WinVerOriginal.Contains("8.1"))
            {
                WinVer = "8.1";


            }
            //Windows 8
            else if (WinVerOriginal.Contains("8"))
            {
                WinVer = "8";


            }
            //Windows 7
            else if (WinVerOriginal.Contains("7"))
            {
                WinVer = "7";


            }
            //Windows Vista
            else if (WinVerOriginal.Contains("Vista"))
            {
                WinVer = "Vista";

            }
            else {
                WinVer = "Unknown";
                Console.WriteLine("You're running a non-supported version of Windows; the application will determine itself.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            Console.WriteLine("WinVer: " + WinVer);
            Console.WriteLine();

        } // gets local Windows version

        private static void checkOnlineVersion()
        {
            Console.WriteLine("CheckOnlineVersion is under construction!");
            OnlineGPUDriverVersion = "220.21"; // debug
        } // (todo) fetch latest NVIDIA GPU driver version

        private static void checkOfflineVersion()
        {
            try
            {
                FileVersionInfo nvvsvcExe = FileVersionInfo.GetVersionInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\System32\nvvsvc.exe"); // Sysnative?
                OfflineGPUDriverVersion = nvvsvcExe.FileDescription.Substring(38).Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Could not find the required NVIDIA executable.");
                Console.ReadKey();
                Environment.Exit(1);
            }
        } // gets current NVIDIA GPU driver version
    }
}