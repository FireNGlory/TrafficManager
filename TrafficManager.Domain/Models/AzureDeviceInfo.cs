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
            DeviceProperties = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("DeviceId", deviceName),
                new KeyValuePair<string, object>("HubEnabledState", true)
            };
            Commands = new List<AzureCommandInfo>
            {
                new AzureCommandInfo { Name = "BringOnline" },
                new AzureCommandInfo { Name = "TakeOffline" },
                new AzureCommandInfo { Name = "Shutdown" }
            };
        }
        public string ObjectType { get; set; }
        public string Version { get; set; }
        public bool IsSimulatedDevice { get; set; }
        public ICollection<KeyValuePair<string, object>> DeviceProperties { get; set; }
        public ICollection<AzureCommandInfo> Commands { get; set; }
    }

}
