using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace TinyNvidiaUpdateChecker.Handlers
{
    class OldMetadataHandler
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

        public static void PrepareCache(bool forceRecache = false)
        {
            var gpuData = GetCachedMetadata("gpu-data.json", forceRecache);
            var osData = GetCachedMetadata("os-data.json", forceRecache);

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

        public static (bool, int) GetGpuIdFromName(string name, bool isNotebook)
        {
            try {
                int gpuId = (int)cachedGPUData[isNotebook ? "notebook" : "desktop"][name];
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

        /// <summary>
        /// Finds the GPU, the version and queries up to date information
        /// </summary>
        public static (GPU, int, bool) GetDriverMetadata(bool forceRecache = false, bool experimental = false)
        {
            bool isNotebook = false;
            bool isDchDriver = false; // TODO rewrite for each GPU
            Regex nameRegex = new(@"(?<=NVIDIA )(.*(?= \([A-Z]+\))|.*(?= [0-9]+GB)|.*(?= with Max-Q Design)|.*(?= COLLECTORS EDITION)|.*)");
            List<int> notebookChassisTypes = [1, 8, 9, 10, 11, 12, 14, 18, 21, 31, 32];
            List<GPU> gpuList = [];
            int osId = 0;

            if (!experimental)
            {
                // Check for notebook
                // TODO rewrite and identify GPUs properly
                if (MainConsole.overrideChassisType == 0)
                {
                    foreach (var obj in new ManagementClass("Win32_SystemEnclosure").GetInstances())
                    {
                        foreach (int chassisType in obj["ChassisTypes"] as ushort[])
                        {
                            isNotebook = notebookChassisTypes.Contains(chassisType);
                        }
                    }
                }
                else
                {
                    isNotebook = notebookChassisTypes.Contains(MainConsole.overrideChassisType);
                }

                // Get operating system ID
                OSClassRoot osData = OldMetadataHandler.RetrieveOSData();
                string osVersion = $"{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}";
                string osBit = Environment.Is64BitOperatingSystem ? "64" : "32";

                if (osVersion == "10.0" && Environment.OSVersion.Version.Build >= 22000)
                {
                    foreach (OSClass os in osData)
                    {
                        if (Regex.IsMatch(os.name, "Windows 11"))
                        {
                            osId = os.id;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (OSClass os in osData)
                    {
                        if (os.code == osVersion && Regex.IsMatch(os.name, osBit))
                        {
                            osId = os.id;
                            break;
                        }
                    }
                }

                if (osId == 0)
                {
                    MainConsole.Write("ERROR!");
                    MainConsole.WriteLine();
                    MainConsole.WriteLine("No NVIDIA driver was found for this operating system configuration. Make sure TNUC is updated.");
                    MainConsole.WriteLine();
                    MainConsole.WriteLine($"osVersion: {osVersion}");
                    MainConsole.callExit(1);
                }
            }


            // Check for DCH for newer drivers
            // TODO do we know if this applies to every GPU?
            using (var regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\nvlddmkm", false))
            {
                if (regKey != null && regKey.GetValue("DCHUVen") != null)
                {
                    isDchDriver = true;
                }
            }

            // Scan computer for GPUs
            foreach (ManagementBaseObject gpu in new ManagementObjectSearcher("SELECT Name, DriverVersion, PNPDeviceID FROM Win32_VideoController").Get())
            {
                string rawName = gpu["Name"].ToString();
                string rawVersion = gpu["DriverVersion"].ToString().Replace(".", string.Empty);
                string pnp = gpu["PNPDeviceID"].ToString();

                // Is it a GPU?
                if (pnp.Contains("&DEV_"))
                {
                    string[] split = pnp.Split("&DEV_");
                    string vendorID = split[0][^4..];
                    string deviceID = split[1][..4];

                    // Are drivers installed for this GPU? If not Windows reports a generic GPU name which is not sufficient
                    if (Regex.IsMatch(rawName, @"^NVIDIA") && nameRegex.IsMatch(rawName))
                    {
                        string gpuName = nameRegex.Match(rawName).Value.Trim().Replace("Super", "SUPER");
                        string cleanVersion = rawVersion.Substring(rawVersion.Length - 5, 5).Insert(3, ".");

                        gpuList.Add(new GPU(gpuName, cleanVersion, vendorID, deviceID, true, isNotebook, isDchDriver));

                        // If experimental mode is enabled, and the vendor is correct, then it's OK to use
                    }
                    else if (vendorID == "10de" && experimental)
                    {
                        gpuList.Add(new GPU(rawName, rawVersion, vendorID, deviceID, true, isNotebook, isDchDriver));
                        // Name does not match but the vendor is NVIDIA, use API to lookup its name
                    }
                    else if (vendorID == "10de" && !experimental)
                    {
                        gpuList.Add(new GPU(rawName, rawVersion, vendorID, deviceID, false, isNotebook, isDchDriver));
                    }
                }
            }

            // If no drivers were found then query PCI Lookup API for each GPU
            // TODO: PCI Lookup API requires seperate GPU name sanitation code which has not been developed yet
            // See issue #215
            Regex apiRegex = new(@"([A-Za-z0-9]+( [A-Za-z0-9]+)+)");

            foreach (GPU gpu in gpuList.Where(x => !x.isValidated))
            {
                string url = $"https://www.pcilookup.com/api.php?action=search&vendor={gpu.vendorId}&device={gpu.deviceId}";
                string rawData = MainConsole.ReadURL(url);
                PCILookupClassRoot apiResponse = JsonConvert.DeserializeObject<PCILookupClassRoot>(rawData);

                if (apiResponse != null && apiResponse.Count > 0)
                {
                    string rawName = apiResponse[0].desc;
                    string rawVersion = gpu.version;

                    if (apiRegex.IsMatch(rawName))
                    {
                        gpu.name = apiRegex.Match(rawName).Value.Trim();
                        gpu.version = rawVersion.Substring(rawVersion.Length - 5, 5).Insert(3, ".");
                        gpu.isValidated = true;
                    }
                }
            }

            // If experimental mode is enabled, do NOT use MetadataHandler, instead, MetadataHandlerExperimental is used,
            // In that case, do not set any GPU as invalid, because it will not pass the code below.
            if (!experimental)
            {
                foreach (GPU gpu in gpuList.Where(x => x.isValidated))
                {
                    (bool success, int gpuId) = OldMetadataHandler.GetGpuIdFromName(gpu.name, gpu.isNotebook);

                    if (success)
                    {
                        gpu.id = gpuId;
                    }
                    else
                    {
                        // check the other type, perhaps it is an eGPU?
                        (success, gpuId) = OldMetadataHandler.GetGpuIdFromName(gpu.name, !gpu.isNotebook);

                        if (success)
                        {
                            gpu.isNotebook = !gpu.isNotebook;
                            gpu.id = gpuId;
                        }
                        else
                        {
                            gpu.isValidated = false;
                        }
                    }
                }
            }

            int gpuCount = gpuList.Where(x => x.isValidated).Count();

            if (gpuCount > 0)
            {
                if (gpuCount > 1)
                {
                    // Validate that the GPU ID is still active on this system
                    int configGpuId = int.Parse(ConfigurationHandler.ReadSetting("GPU ID", gpuList));

                    foreach (GPU gpu in gpuList.Where(x => x.isValidated))
                    {
                        if (gpu.id == configGpuId)
                        {
                            return (gpu, osId, true);
                        }
                    }

                    // GPU ID is no longer active on this system, prompt user to choose new GPU
                    configGpuId = int.Parse(ConfigurationHandler.SetupSetting("GPU ID", gpuList));

                    foreach (GPU gpu in gpuList.Where(x => x.isValidated))
                    {
                        if (gpu.id == configGpuId)
                        {
                            return (gpu, osId, true);
                        }
                    }
                }
                else
                {
                    GPU gpu = gpuList.Where(x => x.isValidated).First();
                    return (gpu, osId, true);
                }
            }

            // If no GPU could be validated, then force recache once, and loop again.
            // This fixes issues related with outdated cache
            if (!forceRecache & !experimental)
            {
                OldMetadataHandler.PrepareCache(true);
                return GetDriverMetadata(true);
            }
            else
            {
                MainConsole.Write("ERROR!");
                MainConsole.WriteLine();
                MainConsole.WriteLine("GPU metadata lookup using OldMetadataHandler failed!");
                MainConsole.WriteLine("Debug information:");
                MainConsole.WriteLine();

                foreach (GPU gpu in gpuList)
                {
                    MainConsole.WriteLine($"GPU Name: '{gpu.name}' | VendorId: {gpu.vendorId} | DeviceId: {gpu.deviceId} | IsNotebook: {gpu.isNotebook}");
                }

                MainConsole.WriteLine();

                // Return success false state
                // This will fall back to NewMetadataHandler
                return (null, 0, false);
            }
        }
        }
}
