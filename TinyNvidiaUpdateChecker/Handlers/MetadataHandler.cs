using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace TinyNvidiaUpdateChecker.Handlers
{
    class MetadataHandler
    {
        /// <summary>
        /// Cache max duration in days
        /// </summary>
        static int cacheDuration = 60;

        /// <summary>
        /// Cached GPU Data
        /// </summary>
        static JObject cachedGPUData;

        /// <summary>
        /// Cached OS Data
        /// </summary>
        static OSClassRoot cachedOSData;

        public static void PrepareCache()
        {
            var gpuData = GetCachedMetadata("gpu-data.json", false);
            var osData = GetCachedMetadata("os-data.json", false);

            // Validate GPU Data JSON
            try {
                cachedGPUData = JObject.Parse(gpuData);
            } catch {
                gpuData = GetCachedMetadata("gpu-data.json", true);
                cachedGPUData = JObject.Parse(gpuData);
            }

            // Validate OS JSON
            try {
                cachedOSData = JsonConvert.DeserializeObject<OSClassRoot>(osData);
            } catch {
                osData = GetCachedMetadata("os-data.json", true);
                cachedOSData = JsonConvert.DeserializeObject<OSClassRoot>(osData);
            }
        }

        public static (bool, int) GetGpuIdFromName(string name, string type)
        {
            try {
                int gpuId = (int)cachedGPUData[type][name];
                return (true, gpuId);
            } catch {
                return (false, 0);
            }
            
        }
        public static OSClassRoot RetrieveOSData() { return cachedOSData; }

        private static dynamic GetCachedMetadata(string fileName, bool forceRecache)
        {
            string dataPath = Path.Combine(ConfigurationHandler.configDirectoryPath, fileName);

            // If the cache exists and is not outdated, then it can be used
            if (File.Exists(dataPath) && !forceRecache) {
                DateTime lastUpdate = File.GetLastWriteTime(dataPath);
                var days = (DateTime.Now - lastUpdate).TotalDays;

                if (days < cacheDuration) {
                    try {
                        return File.ReadAllText(dataPath);
                    } catch {

                    }
                }
            }

            // Delete corrupt/old file if it exists
            if (File.Exists(dataPath)) {
                try {
                    File.Delete(dataPath);
                } catch {
                    // error
                }
            }

            // Download the file and cache it
            string rawData = MainConsole.ReadURL($"{MainConsole.gpuMetadataRepo}/{fileName}");

            try {
                File.AppendAllText(dataPath, rawData);
            } catch {
                // Unable to cache
            }

            return rawData;
        }
    }
}
