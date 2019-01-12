using System;
using System.IO;
using System.Security.Cryptography;

namespace TinyNvidiaUpdateChecker.Handlers
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

    class HashHandler
    {
        /// <summary>
        /// The MD5 hash for HAP v1.8.9
        /// </summary>
        public static string HAP_HASH = "2479dfcf6158f7bc951772a77fcbc3b6";

        /// <summary>
        /// The HAP version currently used
        /// </summary>
        public static string HAP_VERSION = "1.8.9.0";

        /// <summary>
        /// Calcluate the md5 hash of a file, we use it to verify the HTML Aglity Pack DLL so that people don't use the invalid version of it, 
        /// which causes the application to error out.
        /// </summary>
        /// <param name="filename">file name, including filename extention</param>
        /// <returns></returns>
        public static HashInfo CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create()) {
                try {
                    using (var stream = File.OpenRead(filename)) {
                        var hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();

                        return new HashInfo(hash, false);
                    }
                } catch (Exception ex) {
                    Console.Write("ERROR!");
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                    return new HashInfo(null, true);
                }
            }
        }
    }

    class HashInfo
    {
        public String md5;
        public bool error;

        /// <summary>
        /// A information class file that stores md5 hash and a 'is there an' error boolean
        /// </summary>
        /// <param name="md5">The MD5 hash value</param>
        /// <param name="error">has there been any errors?</param>
        public HashInfo(String md5, bool error)
        {
            this.md5 = md5;
            this.error = error;
        }

    }
}
