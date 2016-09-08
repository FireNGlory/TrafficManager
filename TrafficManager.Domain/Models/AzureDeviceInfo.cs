using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficManager.Domain.Models
{
    public class AzureDeviceInfo
    {
        public AzureDeviceInfo()
        {
            
        }

        public AzureDeviceInfo(string deviceName)
        {
            ObjectType = "DeviceInfo";
            Version = "1.0";
            IsSimulatedDevice = false;
            DeviceProperties = new AzureDeviceProperties
            {
                DeviceID = deviceName,
                HubStateEnabled = true,
                DeviceState = "normal",
                FirmwareVersion = "0.1",
                InstalledRam = "8gb",
                Manufacturer = "RyanMack",
                ModelNumber = "RiPi4Way",
                Platform = "Windows IoT Core",
                Processor = "ARM",
                SerialNumber = "12345",
                Latitude = 30.324608,
                Longitude = -81.398250
            };
            Commands = new List<AzureCommandInfo>
            {
                new AzureCommandInfo { Name = "BringOnline", Parameters = new List<AzureCommandParams>()},
                new AzureCommandInfo { Name = "TakeOffline", Parameters = new List<AzureCommandParams>() },
                new AzureCommandInfo { Name = "Shutdown", Parameters = new List<AzureCommandParams>() }
            };
        }
        public string ObjectType { get; set; }
        public string Version { get; set; }
        public bool IsSimulatedDevice { get; set; }
        public AzureDeviceProperties DeviceProperties { get; set; }
        public ICollection<AzureCommandInfo> Commands { get; set; }
    }
}
