using System.Runtime.InteropServices;
using System.Text;

namespace TinyNvidiaUpdateChecker {

    public class iniFile {

        /*
        TinyNvidiaUpdateChecker - Check for NVIDIA desktop GPU drivers, GeForce Experience replacer
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

        public string Path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public iniFile(string INIPath)
        {
            Path = INIPath;
        }

        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.Path);
        }

        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.Path);
            return temp.ToString();

        }
    }
}
