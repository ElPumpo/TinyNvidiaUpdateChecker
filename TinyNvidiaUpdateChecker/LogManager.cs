using System;
using System.Diagnostics;

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
