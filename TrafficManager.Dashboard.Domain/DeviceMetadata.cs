using System;

namespace TrafficManager.Dashboard.Domain
{
    public class DeviceMetadata
    {
        public Guid DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string FriendlyName { get; set; }

        //more stuff here...serial number, dates, usage information, model, etc

    }
    
}
