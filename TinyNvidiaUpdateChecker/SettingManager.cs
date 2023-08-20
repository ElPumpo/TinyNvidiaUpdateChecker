﻿using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace TinyNvidiaUpdateChecker
{

    /// <summary>
    /// Powered by .NET framework "Settings"
    /// </summary>
    class SettingManager
    {

        /// <summary>
        /// Configuration file location, blueprint: <local-appdata><author><project-name>
        /// </summary>
        public static string configFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Hawaii_Beach",
            "TinyNvidiaUpdateChecker",
            "app.config"
        );

        /// <summary>
        /// Check if all the keys are OK before we use them
        /// </summary>
        public static void ConfigInit(string overrideConfigFileLocation)
        {
            if (overrideConfigFileLocation != null) { configFile = overrideConfigFileLocation; }
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configFile);

            if (MainConsole.debug) {
                Console.WriteLine($"configFile: {configFile}");
            }

            // create config file
            if (!File.Exists(configFile)) {
                Console.WriteLine("Generating configuration file.");

                SetupSetting("Check for Updates");
                SetupSetting("Minimal install");
                SetupSetting("Download location");
                SetupSetting("Driver type");

                Console.WriteLine();
            }

            VerifyConfig();

            if (MainConsole.debug) Console.WriteLine();
        }

        /// <summary>
        /// Verify the config before we run the application
        /// </summary>
        private static void VerifyConfig()
        {
            string CHECK_UPDATE = ReadSetting("Check for Updates");
            string MINIMAL_INSTALL = ReadSetting("Minimal install");
            string DOWNLOAD_LOCATION = ReadSetting("Download location");
            string DRIVER_TYPE = ReadSetting("Driver type");

            if (MainConsole.debug) {
                Console.WriteLine($"CHECK_UPDATE: {CHECK_UPDATE}");
                Console.WriteLine($"MINIMAL_INSTALL: {MINIMAL_INSTALL}");
                Console.WriteLine($"DOWNLOAD_LOCATION: {DOWNLOAD_LOCATION}");
                Console.WriteLine($"DRIVER_TYPE: {DRIVER_TYPE}");
            }
        }

        /// <summary>
        /// Reads setting from configuration file, and adds if requested key / value is missing - returns a string.</summary>
        /// <param name="key"> Config key to read value from.</param>
        public static string ReadSetting(string key)
        {
            string result = null;

            try {
                LogManager.Log($"operation='read',key='{key}',val='{ConfigurationManager.AppSettings[key]}'", LogManager.Level.SETTING);

                if (ConfigurationManager.AppSettings[key] != null) {
                    result = ConfigurationManager.AppSettings[key];
                } else {
                    // error reading key
                    Console.WriteLine();
                    Console.WriteLine($"Error reading configuration file, attempting to repair key '{key}' . . .");
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
            string[] values;
            string value;

            switch (key) {

                case "Check for Updates":
                    values = new string[] { "true", "false" };
                    value = SetupConfigYesNoMessagebox("Do you want to search for client updates?", values, "false");
                    break;

                case "Minimal install":
                    values = new string[] { "true", "false" };
                    value = SetupConfigYesNoMessagebox("Do you want to perform a minimal install of the drivers? This will make sure you don't install telemetry and miscellaneous addons, but requires either WinRAR or 7-Zip to be installed.", values, "false");
                    break;

                case "Download location":
                    var locationChooserForm = new LocationChooserForm();
                    value = locationChooserForm.OpenLocationChooserForm();
                    break;

                case "Driver type":
                    TaskDialogButton[] buttons = new TaskDialogButton[] {
                        new("Game Ready Driver (GRD)") { Tag = "grd" },
                        new("Studio Driver (SD)") { Tag = "sd" }
                    };

                    var text = @"If you are a gamer who prioritizes day of launch support for the latest games, patches, and DLCs, choose Game Ready Drivers." +
                            Environment.NewLine + Environment.NewLine +
                            "If you are a content creator who prioritizes stability and quality for creative workflows including video editing, animation, photography, graphic design, and livestreaming, choose Studio Drivers." +
                            Environment.NewLine + Environment.NewLine +
                            "WARNING: not all GPUs support Studio Drivers.";


                    value = ShowButtonDialog("Choose driver type", text, TaskDialogIcon.Information, buttons);
                    break;

                default:
                    MessageBox.Show($"Unknown key '{key}'", "TinyNvidiaUpdateChecker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    value = "unknown";
                    break;
            }

            SetSetting(key, value);
            LogManager.Log($"operation='setup',key='{key}',val='{value}'", LogManager.Level.SETTING);
        }

        private static string SetupConfigYesNoMessagebox(string text, string[] values, string defaultValue)
        {
            if (!MainConsole.confirmDL) {
                DialogResult dialogResult = MessageBox.Show(text, "TinyNvidiaUpdateChecker", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                return (dialogResult == DialogResult.Yes) ? values[0] : values[1];
            }
            else {
                return defaultValue;
            }
        }

        public static string ShowButtonDialog(string title, string text, TaskDialogIcon icon, TaskDialogButton[] buttonList)
        {
            var buttons = new TaskDialogButtonCollection();

            foreach (TaskDialogButton button in buttonList)
            {
                buttons.Add(button);
            }

            TaskDialogPage page = new()
            {
                Heading = title,
                Text = text,
                Buttons = buttons,
                Icon = icon,
                Caption = "TinyNvidiaUpdateChecker"
            };

            TaskDialogButton result = TaskDialog.ShowDialog(page);
            return result.Tag.ToString();
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
