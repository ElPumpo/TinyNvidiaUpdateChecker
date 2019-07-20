using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{

    /*
    TinyNvidiaUpdateChecker - Check for NVIDIA GPU driver updates!
    Copyright (C) 2016-2019 Hawaii_Beach
    
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
    /// Powered by the .NET framework "Settings" function
    /// </summary>
    class SettingManager
    {

        /// <summary>
        /// Direction for configuration folder, blueprint: <local-appdata><author><project-name>
        /// </summary>
        private static string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).CompanyName, FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName);

        public static string configFile = Path.Combine(configDir, "app.config");

        /// <summary>
        /// Check if all the keys are OK before we use them
        /// </summary>
        public static void ConfigInit()
        {

            if (!MainConsole.configSwitch) {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configFile); // set config dir
            } else {
                configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile; // in case we wipe the config (see below)
            }

            ResetConfigMechanism(); // still needed 2017-09-24

            if (MainConsole.debug) {
                Console.WriteLine("configFile: " + AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            }

            // create config file
            if (!MainConsole.configSwitch && !File.Exists(configFile)) {
                Console.WriteLine("Generating configuration file, this only happens once.");

                SetupSetting("Check for Updates");
                SetupSetting("Minimal install");

                Console.WriteLine();
            }

            VerifyConfig();

            if(MainConsole.debug) Console.WriteLine();
        }

        /// <summary>
        /// Verify the config before we run the application
        /// </summary>
        private static void VerifyConfig()
        {
            string CHECK_UPDATE = ReadSetting("Check for Updates");
            string MINIMAL_INSTALL = ReadSetting("Minimal install");

            if(MainConsole.debug) {
                Console.WriteLine("CHECK_UPDATE: " + CHECK_UPDATE);
                Console.WriteLine("MINIMAL_INSTALL: " + MINIMAL_INSTALL);
            }
        }

        /// <summary>
        /// Reads setting from configuration file, and adds if requested key / value is missing - returns a string.</summary>
        /// <param name="key"> Config key to read value from.</param>
        private static string ReadSetting(string key)
        {
            string result = null;

            try {
                LogManager.Log("operation='read',key='" + key + "',val='" + ConfigurationManager.AppSettings[key] + "'", LogManager.Level.SETTING);

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
                Console.WriteLine(ex.ToString());
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
            try {
                LogManager.Log($"operation='set',key='{key}',val='{val}'", LogManager.Level.SETTING);

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
                if (File.Exists(configFile)) {

                    try {
                        File.Delete(configFile);
                    } catch (Exception e) {
                        Console.WriteLine(e.ToString());
                    }
                    LogManager.Log("Wiped config!", LogManager.Level.INFO);
                }

                Console.WriteLine(ex.ToString());
                Console.WriteLine();
                Console.WriteLine("The config file has been wiped due to a possible syntax error, please run the application again and setup your values.");
                if (MainConsole.showUI) Console.ReadKey();
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Ask operator for setting value, not to be confused with setSetting. Only called from setSetting.</summary>
        /// <param name="key"> Requested key name.</param>
        /// <seealso cref="SetSetting(string, string)"> Where settings are made.</seealso>
        private static void SetupSetting(string key)
        {
            string message = null;
            string[] value = null;

            switch (key) {

                // check for update
                case "Check for Updates":
                    message = "Do you want to search for client updates?";
                    value = new string[] { "true", "false" };
                    break;

                // minimal installer maker
                case "Minimal install":
                    message = "Do you want to perform a minimal install of the drivers? This will make sure you don't install telemetry and miscellaneous addons, but requires either WinRAR or 7-Zip to be installed.";
                    value = new string[] { "true", "false" };
                    break;

                default:
                    MessageBox.Show($"Unknown key '{key}'", "TinyNvidiaUpdateChecker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    message = "Unknown";
                    value = null;
                    break;

            }

            if (!MainConsole.confirmDL) {
                DialogResult dialogUpdates = MessageBox.Show(message, "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogUpdates == DialogResult.Yes) {
                    SetSetting(key, value[0]);
                    LogManager.Log($"operation='setup',key='{key}',val='{value[0]}'", LogManager.Level.SETTING);
                } else {
                    SetSetting(key, value[1]);
                    LogManager.Log($"operation='setup',key='{key}',val='{value[1]}'", LogManager.Level.SETTING);
                }
            } else {
                SetSetting(key, value[1]);
                LogManager.Log($"operation='setup',key='{key}',val='{value[1]}'", LogManager.Level.SETTING);
            }

        }

        /// <summary>
        /// Credit goes to Daniel Hilgarth,
        /// fixes the bug where the config strait up refuses to be read
        /// </summary>
        private static void ResetConfigMechanism()
        {
            typeof(ConfigurationManager)
            .GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, 0);

            typeof(ConfigurationManager)
            .GetField("s_configSystem", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, null);

            typeof(ConfigurationManager)
            .Assembly.GetTypes()
            .Where(x => x.FullName ==
               "System.Configuration.ClientConfigPaths")
            .First()
            .GetField("s_current", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, null);
        }

        public static bool ReadSettingBool(string key)
        {
            string read = ReadSetting(key);

            if (read == "true") {
                return true;
            } else if (read == "false") {
                return false;
            } else {

                // setup and read
                SetupSetting(key);
                read = ReadSetting(key);

                if (read == "true") {
                    return true;
                } else if (read == "false") {
                    return false;
                }
            }
            Console.WriteLine($"Could not retrive the key '{key}', this is bad!");
            return false;
        }

    }
}
