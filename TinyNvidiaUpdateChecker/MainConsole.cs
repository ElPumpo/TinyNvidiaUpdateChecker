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

        //Client updater stuff
        private readonly static string UpdateURI = "https://raw.githubusercontent.com/ElPumpo/TinyNvidiaUpdateChecker/master/TinyNvidiaUpdateChecker/version";
        private static int OfflineVer = 1000;
        private static string sOnlineVer;
        private static int OnlineVer;
        private static bool ErrorExists = false;

        //GPU Drivers
        private static string OfflineGPUDriverVersion;
        private static string OnlineGPUDriverVersion;
        private readonly static string GPUversionlink = "https://raw.githubusercontent.com/ElPumpo/TinyNvidiaUpdateChecker/master/TinyNvidiaUpdateChecker/GPUlink";
        private static string GPULinks;


        private static string WinVer;

        static iniFile ini = new iniFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\TinyNvidiaUpdateChecker\" + "config.ini");

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
            Console.WriteLine("TinyNvidiaUpdateChecker launching..");

            CheckForUpdates(); //Updates??
            CheckLocalVersion(); //what's the local version?
            CheckWinVer(); //what's WinVer?
            //CheckOnlineVersion();
            CheckOfflineVersion();

            Console.WriteLine("OfflineGPUDriverVersion: " + OfflineGPUDriverVersion);
            Console.WriteLine("OnlineGPUDriverVersion: " + OnlineGPUDriverVersion);


            //end
            Console.WriteLine();
            Console.WriteLine("Job done! Press any key to exit.");
            Console.ReadKey();
            Application.Exit();


        }

        private static void CheckForUpdates()
        {
            Console.WriteLine("-----UPDATE CHECKER-----");
            Console.WriteLine("Checking for Updates..");
            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(UpdateURI);
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
                    Console.WriteLine("Client is up to date");


                }
                else
                {
                    if (OfflineVer > OnlineVer)
                    {
                        Console.WriteLine("OfflineVer is greater than OnlineVer!");
                    }

                    if (OnlineVer < OfflineVer)
                    {
                        Console.WriteLine("Client is up to date");

                    }
                    else
                    {
                        Console.WriteLine("Update available");
                    }
                }
                //IF end
            }
            Console.WriteLine("OnlineVer: " + OnlineVer);
            Console.WriteLine("OfflineVer: " + OfflineVer);
            Console.WriteLine("----UPDATE CHECKER END--");
        } //Checks for client updates

        private static void CheckLocalVersion()
        {
            string PathToConfig = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\TinyNvidiaUpdateChecker\";
            if (!Directory.Exists(PathToConfig))
            {
                Console.WriteLine("Creating config file, this only happen once per user.");
                Console.WriteLine("CRITICAL: Navigate to" + PathToConfig + "and open Config.ini, ");
                Directory.CreateDirectory(PathToConfig);
                ini.IniWriteValue("Configuration", "CheckForUpdates", "1");
                //ini.IniWriteValue("Configuration", "LocalVersion", "XXX.XX");
                Process.Start("file:///" + PathToConfig + "config.ini");
            }

            if (!File.Exists(PathToConfig + "config.ini"))
            {
                Console.WriteLine("Creating config file, apparently the folder already exist");
                ini.IniWriteValue("Configuration", "CheckForUpdates", "1");
                //ini.IniWriteValue("Configuration", "LocalVersion", "XXX.XX");
                Process.Start("file:///" + PathToConfig + "config.ini");
            }

            /* old code used for GPU detection

            OfflineGPUDriverVersion = ini.IniReadValue("Configuration", "LocalVersion");
            if (OfflineGPUDriverVersion == "XXX.XX")
            {
                Console.WriteLine("Please set the version of your current GPU! If you don't know the current version you may check Task Manager > Information and see description of all NVIDIA processes.");
                Process.Start("file:///" + PathToConfig + "config.ini");
                Console.ReadKey();
                Application.Exit();
            }

            if (OfflineGPUDriverVersion.Length != 6)
            {
                Console.WriteLine("Invalid length of LocalVersion!");
                Console.WriteLine("Current LocalVersion: " + OfflineGPUDriverVersion);
                Console.WriteLine("Template: XXX.XX");
                Process.Start("file:///" + PathToConfig + "config.ini");
                Application.Exit();
            }
            */


        } //Sets local GPU driver version

        private static void CheckWinVer()
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

        } //Gets local Windows version

        private static void CheckOnlineVersion()
        {

            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(GPUversionlink);
                StreamReader reader = new StreamReader(stream);
                GPULinks = reader.ReadToEnd();
                reader.Close();
                stream.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Could not search for GPU drivers, check your internet connection!");
                Console.ReadKey();
                Application.Exit();
            }

            string NVIDIALink;

            if (WinVer == "10")
            {
                if (Environment.Is64BitOperatingSystem == true)
                {
                    Console.WriteLine(GPULinks);
                    //NVIDIALink = File.ReadLines(GPULinks).Skip(2).First();
                }
                else {
                    NVIDIALink = "10.x86";
                }
            }
            else {
                //Windows 8, 8.1, 7 and Vista share the same graphics driver
                if (Environment.Is64BitOperatingSystem == true)
                {
                    NVIDIALink = "other.amd64";
                }
                else
                {
                    NVIDIALink = "other.x86";
                }
            }

            //Console.WriteLine(NVIDIALink);

            //Debug






        } //Gets latest GPU driver version


        private static void CheckOfflineVersion()
        {
            
        }

    }
}

