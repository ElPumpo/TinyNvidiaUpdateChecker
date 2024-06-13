using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TinyNvidiaUpdateChecker.Handlers
{
    public class ComponentHandler
    {
        public static List<Component> ParseComponentData(string driverRootPath)
        {
            List<Component> components = [];
            XmlDocument doc = new();

            foreach (string dir in Directory.GetDirectories(driverRootPath))
            {
                string nviFile = FindNviFile(dir);

                if (nviFile != null)
                {
                    doc.Load(nviFile);
                    string name = Path.GetFileName(dir);
                    string label = FindNviLabel(doc, name);

                    componentLabel[name] = label;
                    Dictionary<string, string> dependencies = FindNviDependencies(doc);

                    Component component = new(name, label, dependencies);
                    components.Add(component);
                }
            }

            // Populate dependency labels
            foreach (Component component in components)
            {
                foreach (KeyValuePair<string, string> dependency in component.dependencies)
                {
                    string label = GetComponentLabelFromName(dependency.Key);
                    component.dependencies[dependency.Key] = label;
                }
            }

            return components;
        }

        static Dictionary<string, string> FindNviDependencies(XmlDocument doc)
        {
            Dictionary<string, string> dependencies = [];

            try
            {
                XmlNodeList dependenciesNode = doc.SelectNodes("nvi/dependencies");

                foreach (XmlNode packages in dependenciesNode)
                {
                    if (packages != null && packages.ChildNodes.Count > 0)
                    {
                        foreach (XmlNode node in packages.ChildNodes)
                        {
                            XmlElement package = (XmlElement)node;
                            string type = package.GetAttributeNode("type").InnerText;

                            if (type == "requires")
                            {
                                string name = package.GetAttributeNode("package").InnerText;
                                // Check for component requirement override names as some do not match the component dir name
                                string overrideName = ValidateNviComponentName(name);
                                dependencies.TryAdd(overrideName, "");
                            }
                        }

                    }
                }
            } catch { }

            return dependencies;
        }

        static string ValidateNviComponentName(string name)
        {
            if (dependencyNameOverride.ContainsKey(name))
            {
                return dependencyNameOverride[name];
            }

            return name;
        }

        static string FindNviLabel(XmlDocument doc, string componentName)
        {
            if (componentLabelOverride.ContainsKey(componentName))
            {
                return componentLabelOverride[componentName];
            }

            try
            {
                XmlNodeList stringsNode = doc.SelectNodes("nvi/strings/localized");

                foreach (XmlNode node in stringsNode)
                {
                    XmlElement element = (XmlElement)node;
                    var stringNodes = element.GetElementsByTagName("string");

                    foreach (XmlElement child in stringNodes)
                    {
                        string type = child.GetAttributeNode("name").InnerText;

                        if (type == "title" || type == "Title")
                        {
                            return child.GetAttributeNode("value").InnerText;
                        }
                    }
                }

                stringsNode = doc.SelectNodes("nvi/strings");
                foreach (XmlNode node in stringsNode)
                {
                    XmlElement element = (XmlElement)node;
                    var stringNodes = element.GetElementsByTagName("string");

                    foreach (XmlElement child in stringNodes)
                    {
                        string type = child.GetAttributeNode("name").InnerText;

                        if (type == "title" || type == "Title")
                        {
                            return child.GetAttributeNode("value").InnerText;
                        }
                    }
                }
            } catch { }

            return componentName;
        }

        static string FindNviFile(string dir)
        {
            string dirName = Path.GetFileName(dir);
            string[] fileNames = [
                $@"{dir}\{dirName}.nvi",
                $@"{dir}\{dirName.Replace(".", "")}.nvi"
            ];

            foreach (string file in fileNames) {
                if (File.Exists(file)) {
                    return file;
                }
            }

            foreach (string file in Directory.GetFiles(dir)) {
                if (file.EndsWith(".nvi")) {
                    return file;
                }
            }

            return null;
        }

        public static string GetComponentLabelFromName(string searchName)
        {
            if (componentLabel.ContainsKey(searchName))
            {
                return componentLabel[searchName];
            }

            return searchName;
        }
        public static string GetComponentDescription(string name)
        {
            if (componentDescription.ContainsKey(name))
            {
                return componentDescription[name];
            }
            else
            {
                return "Unknown component, there is no description for it.";
            }
        }

        public static Dictionary<string, string> componentDescription = new() {
            {"Display.Driver", "This is the graphics driver"},
            {"Display.Nview", "RTX Desktop Manager is a tool designed to enhance productivity and workflow for professionals.\n\nYou can define custom regions on your desktop and easily snap application windows into those regions, as well as create custom user profiles that organize your windows and applications on launch.\n\nOnly compatiable with RTX and Quadro GPUs."},
            {"Display.Optimus", "NVIDIA Optimus is a technology designed to enhance the battery life of laptops by dynamically switching between two graphics processing units (GPUs) based on the tasks being performed. Install it if you're on notebook."},
            {"Display.Update", "This component is the NVIDIA driver update checker. It notifies you when a new release is available. But you're using TNUC instead, right?"},
            {"FrameViewSDK", "The NVIDIA FrameView SDK is a set of tools designed for capturing and analyzing performance metrics in real-time for gaming and other graphics-intensive applications."},
            {"GFExperience", "GeForce Experience is a companion application for NVIDIA GeForce graphics cards, designed to enhance the gaming experience through various features and tools."},
            {"GFExperience.NvStreamSrv", "This component is the live streaming library for ShadowPlay. Without it, you can only record to disk."},
            {"HDAudio", "The drivers to send audio over HDMI and DisplayPort on NVIDIA GPUs."},
            {"MSVCRT", "Runtimes provided by Microsoft. Some components require them, but are not required for minimal driver install.\n\nIf you just reinstalled Windows then it's recommended to choose it."},
            {"nodejs", "A GeForce Experience library required for it to function"},
            {"NvAbHub", "Undocumented component, some other components require it"},
            {"NvBackend", "Background service that runs alongside NVIDIA GeForce Experience. Its primary function is to manage various tasks related to the GeForce Experience application and its functionalities."},
            {"NvCamera", "NVIDIA Ansel is a in-game photography tool that allows capturing screenshots from supported games with customizable camera settings and effects."},
            {"NvContainer", "The background service for some components."},
            {"NvModuleTracker", "Process and module monitoring driver."},
            {"NVPCF", "This component is a driver for very specific laptops, such as some Asus and Acer models.\n\nOnly install this component if you have an unknown device in Device Manager with hardware id 'ACPI\\NVDA0820'"},
            {"NvTelemetry", "This component collects your data without consent and sends them to NVIDIA."},
            {"NvVAD", "NVIDIA Virtual Audio driver, required for ShadowPlay to record audio."},
            {"NvvHCI", "NVIDIA Shield Controller Wired Driver"},
            {"NVWMI", "NVIDIA Enterprise Management Toolkit lets IT administrators create scripts and programs for many administrative tasks and functions such as configuring GPU settings, retrieving GPU information, and performing automated tasks.\n\nThis can only be installed on workstation entry cards (Quadro, NVS)"},
            {"PhysX", "PhysX is a physics engine developed by NVIDIA for use in video games and other real-time simulation applications.\n\nOutdated technology, if you only play modern video games then you don't need it."},
            {"PPC", "This component is a specific USB-C driver used for a technology called VirtualLink. It provides a method of pairing VR headsets with computers, but has been abandoned."},
            {"ShadowPlay", "ShadowPlay is the popular NVIDIA tool that let's you record your gameplay using their NvENC encoder."},
            {"ShieldWirelessController", "NVIDIA Shield Controller Wireless Driver."},
            {"Update.Core", "Component that allows GeForce Expereince to self update."}
        };

        public static Dictionary<string, string> componentLabel = [];

        public static Dictionary<string, string> componentLabelOverride = new() {
            {"Display.Optimus", "NVIDIA Optimus"},
            {"Display.Update", "NVIDIA Driver Update Checker"},
            {"MSVCRT", "Microsoft C Runtimes 2017 & 2019"},
            {"NVPCF", "NV Platform Controller"},
            {"NvvHCI", "NVIDIA Shield Controller Wired Driver"},
            {"NvModuleTracker", "Process Monitor"},
            {"ShieldWirelessController", "NVIDIA Shield Controller Wireless Driver"},
            {"Update.Core", "GFE Updater"}
        };

        public static Dictionary<string, string> dependencyNameOverride = new() {
            {"Display.GFExperience", "GFExperience"},
            {"NvNodejs", "nodejs"},
            {"NvContainer.MessageBus", "NvContainer"},
            {"NvContainer.LocalSystem", "NvContainer"},
            {"NvContainer.ServiceUser", "NvContainer"},
            {"NvContainer.Session", "NvContainer"},
            {"NvContainer.NvapiMonitor", "NvContainer"},
            {"NvContainer.User", "NvContainer"},
            {"NvContainerLocalSystem" ,"NvContainer"},
            {"VirtualAudio.Driver", "NvVAD"},
            {"GpxCommon.Oss", "Display.Driver"}
        };
    }

    public class Component(string name, string label, Dictionary<string, string> dependencies)
    {
        public string name = name;
        public string label = label;
        public Dictionary<string, string> dependencies = dependencies;
        public int index;
    }
}
