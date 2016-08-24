using System;
using System.Diagnostics;

namespace TinyNvidiaUpdateChecker
{
    class LogManager
    {
        public enum Level
        {
            INFO,
            ERROR,
            SETTING
        }

        [STAThread]
        public static void log(string information, LogManager.Level level)
        {
            /// The accual fuck? without this under, if using Debug.WriteLine in multiple classes the settings will be unable to be read. They will be returned as null and the application falls to its knees.
            /// Looking at AppDomain.CurrentDomain.SetupInformation.ConfigurationFile it seems that the config path isn't set, trying to set it here too solves the issue. For now...
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", MainConsole.fullConfig);


            string logMessage = null;
            switch (level) {

                // INFO
                case Level.INFO:
                    logMessage = "[INFO] " + information;
                    break;

                // ERROR
                case Level.ERROR:
                    logMessage = "[ERROR] " + information;
                    break;

                // SETTING
                case Level.SETTING:
                    logMessage = "[SETTING] " + information;
                    break;

                default:
                    break;
            }
            Debug.WriteLine(logMessage);

        }
    }
}
