using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

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

    /// <summary>
    /// Everything related to reading settings from config
    /// </summary>
    class SettingManager
    {
        /// <summary>
        /// Reads setting from configuration file, and adds if requested key / value is missing - returns a string.</summary>
        /// <param name="key"> Config key to read value from.</param>
        public static string readSetting(string key)
        {
            string result = null;

            try {
                LogManager.log("key='" + key + "',val='" + ConfigurationManager.AppSettings[key] + "'", LogManager.Level.SETTING);

                if (ConfigurationManager.AppSettings[key] != null) {
                    result = ConfigurationManager.AppSettings[key];
                } else {

                    // error reading key
                    Console.WriteLine();
                    Console.WriteLine("Error reading configuration file, attempting to repair key '" + key + "' . . .");
                    setupSetting(key);

                    result = ConfigurationManager.AppSettings[key];
                }
            }
            catch (ConfigurationErrorsException ex) {
                Console.WriteLine(ex.StackTrace);
                LogManager.log(ex.ToString(), LogManager.Level.ERROR);
                Console.WriteLine();
            }

            return result;
        }

        /// <summary>
        /// Set / update setting in configuration.</summary>
        /// <param name="key"> Requested key name.</param>
        /// <param name="val"> Requested value.</param>
        public static void setSetting(string key, string val)
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
                    LogManager.log("Wiped config!", LogManager.Level.INFO);
                }

                Console.WriteLine(ex.StackTrace);
                LogManager.log(ex.ToString(), LogManager.Level.ERROR);
                Console.WriteLine();
                Console.WriteLine("The config file has been wiped due to a possible syntax error, please run the application again and setup your values.");
                if (MainConsole.showUI == true) Console.ReadKey();
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Ask operator for setting value, not to be confused with setSetting. Only called from setSetting.</summary>
        /// <param name="key"> Requested key name.</param>
        /// <seealso cref="setSetting(string, string)"> Where settings are made.</seealso>
        public static void setupSetting(string key)
        {
            string message = null;
            string[] value = null;

            switch (key)
            {

                // check for update
                case "Check for Updates":
                    message = "Do you want to search for client updates?";
                    value = new string[] { "true", "false" };
                    break;

                // gpu
                case "GPU Type":
                    message = "If you're running a desktop GPU select Yes, if you're running a mobile GPU select No.";
                    value = new string[] { "desktop", "mobile" };
                    break;

                // desc
                case "Show Driver Description":
                    message = "Do you want to see the driver description? (BETA)";
                    value = new string[] { "true", "false" };
                    break;

                default:
                    MessageBox.Show("Unknown key '" + key + "'", "TinyNvidiaUpdateChecker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    message = "Unknown";
                    value = null;
                    break;

            }

            DialogResult dialogUpdates = MessageBox.Show(message, "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogUpdates == DialogResult.Yes)
            {
                setSetting(key, value[0]);
            }
            else
            {
                setSetting(key, value[1]);
            }

        }
    }
}
