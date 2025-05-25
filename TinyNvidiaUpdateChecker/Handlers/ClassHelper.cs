
using System;
using System.Collections.Generic;

public class OSClass
{
    public string code { get; set; }
    public string name { get; set; }
    public int id { get; set; }
}

public class GPU(string name, string version, string vendorId, string deviceId, bool isValidated, bool isNotebook, bool isDch)
{
    public string name { get; set; } = name;
    public string version { get; set; } = version;
    public string vendorId { get; set; } = vendorId;
    public string deviceId { get; set; } = deviceId;
    public int id { get; set; } = 0;
    public bool isValidated { get; set; } = isValidated;
    public bool isNotebook { get; set; } = isNotebook;
    public bool isDch { get; set; } = isDch;
}

public class DriverMetadata(string name, string version, long fileSize, string platform, string downloadUrl, string pdfUrl, string releaseNotes, DateTime releaseDate)
{
    public DriverMetadata() : this(null, null, 0, null, null, null, null, new DateTime()) {}

    public string name { get; set; } = name;
    public string version { get; set; } = version;
    public long fileSize { get; set; } = fileSize;
    public string platform { get; set; } = platform;
    public string downloadUrl { get; set; } = downloadUrl;
    public string pdfUrl { get; set; } = pdfUrl;
    public string releaseNotes { get; set; } = releaseNotes;
    public DateTime releaseDate { get; set; } = releaseDate;
}

public class OSClassRoot : List<OSClass> { }

public class PCILookupClassRoot : List<PCILookupClass> { }

public class PCILookupClass
{
    public string id { get; set; }
    public string desc { get; set; }
    public string venID { get; set; }
    public string venDesc { get; set; }
}

public class GitHubAPIReleaseRoot
{
    public string url { get; set; }
    public string assets_url { get; set; }
    public string upload_url { get; set; }
    public string html_url { get; set; }
    public int id { get; set; }
    public dynamic author { get; set; }
    public string node_id { get; set; }
    public string tag_name { get; set; }
    public string target_commitish { get; set; }
    public string name { get; set; }
    public bool draft { get; set; }
    public bool prerelease { get; set; }
    public DateTime created_at { get; set; }
    public DateTime published_at { get; set; }
    public Asset[] assets { get; set; }
    public string tarball_url { get; set; }
    public string zipball_url { get; set; }
    public string body { get; set; }
    public dynamic reactions { get; set; }
}

public class Asset
{
    public string url { get; set; }
    public int id { get; set; }
    public string node_id { get; set; }
    public string name { get; set; }
    public object label { get; set; }
    public dynamic uploader { get; set; }
    public string content_type { get; set; }
    public string state { get; set; }
    public int size { get; set; }
    public int download_count { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public string browser_download_url { get; set; }
}
