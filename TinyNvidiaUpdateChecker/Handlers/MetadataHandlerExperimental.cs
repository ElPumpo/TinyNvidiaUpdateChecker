using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using TinyNvidiaUpdateChecker;

public class GpuDevice
{
    public string id { get; set; }
    public string vendorid { get; set; }
    public string ssid { get; set; }
    public string svid { get; set; }
    public string name { get; set; }
}

public class DriverVersion
{
    public string key { get; set; }
    public string version { get; set; }
    public List<string> os { get; set; }
    public string bit { get; set; }
    public string type { get; set; }
    public int dch { get; set; }
    public List<int> supports { get; set; }
}

public class CombinedGpuData
{
    public Dictionary<string, GpuDevice> devices { get; set; }
    public List<DriverVersion> versions { get; set; }
}

public class MetadataHandlerExperimental
{
    private static CombinedGpuData _combinedGpuData;

    public static bool LoadCombinedJsonData()
    {
        try
        {
            string jsonString = MainConsole.ReadURL(MainConsole.experimentalGpuMetadataRepo);
            _combinedGpuData = JsonSerializer.Deserialize<CombinedGpuData>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static (GpuDevice matchedGpu, int deviceKey) FindGpuDetailsByDeviceId(string deviceId)
    {
        foreach (KeyValuePair<string, GpuDevice> entry in _combinedGpuData.devices)
        {
            GpuDevice device = entry.Value;
            if (device.id.Equals(deviceId, StringComparison.OrdinalIgnoreCase))
            {
                return (device, int.Parse(entry.Key));
            }
        }
        return (null, 0);
    }

    public static DriverVersion FindLatestDriverForGpu(int gpuIndex, string driverType)
    {
        DriverVersion latestDriver = null;
        Version latestParsedVersion = new(0, 0);

        foreach (var driver in _combinedGpuData.versions)
        {
            if (driver.supports.Contains(gpuIndex))
            {
                // Does driver type match?
                if ((driverType == "sd" && driver.type == "Studio") || driverType != "sd")
                {
                    if (Version.TryParse(driver.version, out Version currentParsedVersion))
                    {
                        if (currentParsedVersion > latestParsedVersion)
                        {
                            latestParsedVersion = currentParsedVersion;
                            latestDriver = driver;
                        }
                    }
                }
            }
        }
        return latestDriver;
    }

    public static (DriverMetadata metadata, string errorCode) GetDriverMetadata(string deviceId, string driverType)
    {
        if (!LoadCombinedJsonData()) return (null, "Error parsing GPU metadata json.");
        (GpuDevice matchedGpu, int gpuIndex) = FindGpuDetailsByDeviceId(deviceId);

        if (matchedGpu != null)
        {
            DriverVersion latestDriver = FindLatestDriverForGpu(gpuIndex, driverType);

            if (latestDriver != null)
            {
                string downloadUrl = $"https://international.download.nvidia.com/Windows/{latestDriver.version}/{latestDriver.key}.exe";
                string pdfUrl = $"https://international.download.nvidia.com/Windows/{latestDriver.version}/{latestDriver.version}-win11-win10-release-notes.pdf";

                // Query release date and file size
                using (var request = new HttpRequestMessage(HttpMethod.Head, downloadUrl))
                {
                    using var response = MainConsole.httpClient.Send(request);
                    response.EnsureSuccessStatusCode();

                    // File size
                    long fileSize = response.Content.Headers.ContentLength.Value;

                    // Release date
                    DateTimeOffset? releaseDateOffset = response.Content.Headers.LastModified;
                    DateTime releaseDate = (DateTime)(releaseDateOffset?.LocalDateTime);

                    // Test if PDF url is OK
                    if (!IsUrlOk(pdfUrl)) pdfUrl = null;

                    // Release notes
                    string releaseNotes = RetrieveReleaseNotes();

                    return (new DriverMetadata(latestDriver.key, latestDriver.version, fileSize, latestDriver.type, downloadUrl, pdfUrl, releaseNotes, releaseDate), null);
                }
            }
            else
            {
                // TODO implement popup, and ask to revert to GRD
                string error = "No compatible driver was found for your GPU.";
                if (driverType == "sd")
                {
                    error += "\nYou have opted to only recieve Studio drivers. Perhaps your GPU has no available Studio drivers.";
                }
                return (null, error);
            }
        }
        else
        {
            return (null, $"Your GPU is not supported by this experimental repo. Your device ID: {deviceId}");
        }
    }

    private static bool IsUrlOk(string url)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = MainConsole.httpClient.Send(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string RetrieveReleaseNotes()
    {
        try
        {
            // Parse code
            string releaseNotes = MainConsole.ReadURL(MainConsole.experimentalGpuMetadataRepoReleaseNotes);
            JObject parsed = JObject.Parse(releaseNotes);
            string html = parsed["result"].ToString();

            // Remove download forms
            html = Regex.Replace(html, @"<form[^>]*action\s*=\s*[""']?/download/.*?</form>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Only get the three latest release notes
            MatchCollection matches = Regex.Matches(html, @"<div class=""version[^""]*"" id=""changes-[^""]+"">.*?<\/div>", RegexOptions.Singleline);
            string limitedHtml = "";

            for (int i = 0; i < Math.Min(3, matches.Count); i++)
            {
                limitedHtml += matches[i].Value;
            }

            string finalHtml = "<html><head><meta charset=\"UTF-8\"></head><body>" + limitedHtml + "</body></html>";
            return finalHtml;
        }
        catch
        {
            return "Unable to retireve release notes.";
        }
    }
}