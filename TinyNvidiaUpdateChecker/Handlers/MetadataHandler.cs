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
        public static (JObject, OSClassRoot) RetrieveMetadata(string gpuName, bool isNotebook)
        {
            string gpuDataRaw = GetCachedMetadata("gpu-data.json", false);
            string osDataRaw = GetCachedMetadata("os-data.json", false);
            JObject gpuData;
            OSClassRoot osData;

            // Validate GPU JSON
            try {
                gpuData = JObject.Parse(gpuDataRaw);
            } catch {
                gpuDataRaw = GetCachedMetadata("gpu-data.json", true);
                gpuData = JObject.Parse(gpuDataRaw);
            }

            // If the cached metadata does not contain current GPU then force recache
            if (gpuName != null) {
                try {
                    int gpuId = (int)gpuData[isNotebook ? "notebook" : "desktop"][gpuName];
                } catch {
                    gpuDataRaw = GetCachedMetadata("gpu-data.json", true);
                    gpuData = JObject.Parse(gpuDataRaw);
                }
            }


            // Validate OS JSON
            try {
                osData = JsonConvert.DeserializeObject<OSClassRoot>(osDataRaw);
            } catch {
                osDataRaw = GetCachedMetadata("os-data.json", true);
                osData = JsonConvert.DeserializeObject<OSClassRoot>(osDataRaw);
            }

            return (gpuData, osData);
        }

        private static string GetCachedMetadata(string fileName, bool forceRecache)
        {
            string dataPath = Path.Combine(ConfigurationHandler.configDirectoryPath, fileName);

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

            // Delete corrupt file if it exists
            if (File.Exists(dataPath)) {
                try {
                    File.Delete(dataPath);
                } catch {
                    // error
                }
            }

            // Cache the file
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
