using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{

    /*
    TinyNvidiaUpdateChecker - Check for NVIDIA GPU drivers, GeForce Experience replacer
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

    /// <summary>
    /// Everything related to reading settings from config
    /// </summary>
    class SettingManager
    {
        /// <summary>
        /// Reads setting from configuration file, and adds if requested key / value is missing - returns a string.</summary>
        /// <param name="key"> Config key to read value from.</param>
        public static string ReadSetting(string key)
        {
            string result = null;

            try {
                LogManager.Log("key='" + key + "',val='" + ConfigurationManager.AppSettings[key] + "'", LogManager.Level.SETTING);

                if (ConfigurationManager.AppSettings[key] != null) {
                    result = ConfigurationManager.AppSettings[key];
                } else {

                    // error reading key
                    Console.WriteLine();
                    Console.WriteLine("Error reading configuration file, attempting to repair key '" + key + "' . . .");
                    SetupSetting(key);

                    result = ConfigurationManager.AppSettings[key];
                }
            }
            catch (ConfigurationErrorsException ex) {
                Console.WriteLine(ex.StackTrace);
                LogManager.Log(ex.ToString(), LogManager.Level.ERROR);
                Console.WriteLine();
            }

            return result;
        }

        /// <summary>
        /// Set / update setting in configuration.</summary>
        /// <param name="key"> Requested key name.</param>
        /// <param name="val"> Requested value.</param>
        public static void SetSetting(string key, string val)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;

                // check if already in config
                if (settings[key] == null) {
                    settings.Add(key, val);
                } else {
                    settings[key].Value = val;
                }

                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

            } catch (ConfigurationErrorsException ex) {

                // clean config file
                if (File.Exists(MainConsole.fullConfig)) {

                    try {
                        File.Delete(MainConsole.fullConfig);
                    } catch (Exception e) {
                        Console.WriteLine(e.StackTrace);
                    }
                    LogManager.Log("Wiped config!", LogManager.Level.INFO);
                }

                Console.WriteLine(ex.StackTrace);
                LogManager.Log(ex.ToString(), LogManager.Level.ERROR);
                Console.WriteLine();
                Console.WriteLine("The config file has been wiped due to a possible syntax error, please run the application again and setup your values.");
                if (MainConsole.showUI == true) Console.ReadKey();
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Ask operator for setting value, not to be confused with setSetting. Only called from setSetting.</summary>
        /// <param name="key"> Requested key name.</param>
        /// <seealso cref="SetSetting(string, string)"> Where settings are made.</seealso>
        public static void SetupSetting(string key)
        {
            string message = null;
            string[] value = null;
            Boolean special = false;

            switch (key)
            {

                // check for update
                case "Check for Updates":
                    message = "Do you want to search for client updates?";
                    value = new string[] { "true", "false" };
                    special = false;
                    break;

                // gpu type
                case "GPU Type":
                    message = "If you're running a desktop GPU select Yes, if you're running a mobile GPU select No.";
                    value = new string[] { "desktop", "mobile" };
                    special = false;
                    break;

                // desc
                case "Show Driver Description":
                    message = "Do you want to see the driver description? (BETA)";
                    value = new string[] { "true", "false" };
                    special = false;
                    break;

                // the gpu
                case "GPU Name":
                    message = "GPU Name";
                    value = new string[] { SelectGPU.getGPU() };
                    special = true;
                    break;


                // minimal installer maker
                case "Minimal install":
                    message = "Do you want to perform a minimal install of the drivers? This will remove telemetry and other things you won't need, but requires WinRAR installed.";
                    value = new string[] { "true", "false" };
                    special = false;
                    break;

                default:
                    MessageBox.Show("Unknown key '" + key + "'", "TinyNvidiaUpdateChecker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    message = "Unknown";
                    value = null;
                    special = false;
                    break;

            }
            if(special) {
                SetSetting(key, value[0]);
            } else {
                DialogResult dialogUpdates = MessageBox.Show(message, "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogUpdates == DialogResult.Yes) {
                    SetSetting(key, value[0]);
                } else {
                    SetSetting(key, value[1]);
                }
            }

        }

    }
}
