
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
    public int gpuId { get; set; } = 0;
    public bool isValidated { get; set; } = isValidated;
    public bool isNotebook { get; set; } = isNotebook;
    public bool isDch { get; set; } = isDch;
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
