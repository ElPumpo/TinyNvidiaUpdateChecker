using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{
    class MainConsole
    {
        private readonly static string UpdateURI = "";

        private static int OfflineVer = 1000;
        private static int OnlineVer;

        //GPU Drivers
        private static string OfflineGPUDriverVersion; //359.00
        private static string OnlineGPUDriverVersion;

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
                if (Array.IndexOf(parms, "-hidden") != -1)
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


            CheckLocalVersion(); //what's the local version?
            CheckWinVer(); //what's WinVer?
            CheckOnlineVersion();


            //end
            Console.WriteLine();
            Console.WriteLine("Job done! Press any key to exit.");
            Console.ReadKey();
            Application.Exit();


        }

        private static void CheckForUpdates()
        {
            try
            {
                using (var request = new WebClient())
                {
                    //Download the data
                    var requestData = request.DownloadData(UpdateURI);

                    //Return the data by encoding it back to text!
                    return Encoding.ASCII.GetString(requestData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

            

        }
        private static void CheckLocalVersion()
        {
            string PathToConfig = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\TinyNvidiaUpdateChecker\";
            if (!Directory.Exists(PathToConfig))
            {
                Console.WriteLine("Creating config file, this only happen once per user.");
                Console.WriteLine("CRITICAL: Navigate to" + PathToConfig + "and open Config.ini, ");
                Directory.CreateDirectory(PathToConfig);
                ini.IniWriteValue("Configuration", "CheckForUpdates", "1");
                ini.IniWriteValue("Configuration", "LocalVersion", "XXX.XX");
                Process.Start("file:///" + PathToConfig + "config.ini");
            }

            if (!File.Exists(PathToConfig + "config.ini"))
            {
                Console.WriteLine("Creating config file, apparently the folder already exist");
                ini.IniWriteValue("Configuration", "CheckForUpdates", "1");
                ini.IniWriteValue("Configuration", "LocalVersion", "XXX.XX");
                Process.Start("file:///" + PathToConfig + "config.ini");
            }

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

            Console.WriteLine("OfflineGPUDriverVersion: " + OfflineGPUDriverVersion);

        }
        private static void CheckWinVer()
        {
            string WinVerOriginal = Environment.OSVersion.ToString();

            //Windows 10
            if (WinVerOriginal.Contains("10"))
            {
                WinVer = "10";

                //Windows 8.1
            }
            else if (WinVerOriginal.Contains("8.1"))
            {
                WinVer = "8.1";

                //Windows 8
            }
            else if (WinVerOriginal.Contains("8"))
            {
                WinVer = "8";

                //Windows 7
            }
            else if (WinVerOriginal.Contains("7"))
            {
                WinVer = "7";

                //Windows Vista
            }
            else if (WinVerOriginal.Contains("Vista"))
            {
                WinVer = "Vista";

            }else{
                WinVer = "Unknown";
                Console.WriteLine("You're running a non-supported version of Windows; the application will determine itself.");
                Console.ReadKey();
                Application.Exit();
            }

            Console.WriteLine("WinVer: " + WinVer);

        }
        private static void CheckOnlineVersion()
        {
            string NVIDIALink;

            if (WinVer == "10")
            {
                if (Environment.Is64BitOperatingSystem == true)
                {
                    NVIDIALink = "http://www.nvidia.com/download/driverResults.aspx/95705";
                }else{
                    NVIDIALink = "http://www.nvidia.com/download/driverResults.aspx/95687";
                }
            }else{
                //Windows 8, 8.1, 7 and Vista share the same graphics driver
                if (Environment.Is64BitOperatingSystem == true)
                {
                    NVIDIALink = "http://www.nvidia.com/download/driverResults.aspx/95597";
                }
                else
                {
                    NVIDIALink = "http://www.nvidia.com/download/driverResults.aspx/95579";
                }



            }


            }
        }
}

