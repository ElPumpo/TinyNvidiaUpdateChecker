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

        public static void Log(string msg, Level level)
        {
            string logMessage = null;

            switch (level) {

                // INFO
                case Level.INFO:
                    logMessage = "[INFO] " + msg;
                    break;

                // ERROR
                case Level.ERROR:
                    logMessage = "[ERROR] " + msg;
                    break;

                // SETTING
                case Level.SETTING:
                    logMessage = "[SETTING] " + msg;
                    break;

                default:
                    break;
            }

            Debug.WriteLine(logMessage);
        }

        public static void Log(string information)
        {
            Debug.WriteLine("[INFO] " + information);
        }
    }
}
