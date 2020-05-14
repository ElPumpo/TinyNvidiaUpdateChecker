using System;
using System.IO;
using System.Security.Cryptography;

namespace TinyNvidiaUpdateChecker.Handlers
{

    class HashHandler
    {
        /// <summary>
        /// The MD5 hash for HAP v1.11.23
        /// </summary>
        public static string HAP_HASH = "24-1D-D8-58-41-D3-4F-88-92-3C-5A-BA-FD-D6-56-FB";

        /// <summary>
        /// The HAP version currently used
        /// </summary>
        public static string HAP_VERSION = "1.11.23.0";

        /// <summary>
        /// Calcluate the md5 hash of a file, we use it to verify the HTML Aglity Pack DLL so that people don't use the invalid version of it, 
        /// which causes the application to error out.
        /// </summary>
        /// <param name="fileName">file name, including filename extention</param>
        /// <returns></returns>
        public static HashInfo CalculateMD5(string fileName)
        {
            using (var md5 = MD5.Create()) {
                try {
                    using (var stream = File.OpenRead(fileName)) {
                        var hash = BitConverter.ToString(md5.ComputeHash(stream));

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
        public string md5;
        public bool error;

        /// <summary>
        /// A information class file that stores md5 hash and a 'is there an' error boolean
        /// </summary>
        /// <param name="md5">The MD5 hash value</param>
        /// <param name="error">has there been any errors?</param>
        public HashInfo(string md5, bool error) {
            this.md5 = md5;
            this.error = error;
        }
    }
}
