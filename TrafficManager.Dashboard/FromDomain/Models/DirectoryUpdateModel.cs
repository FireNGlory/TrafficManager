using System;

namespace TrafficManager.Domain.Models
{
    public class DirectoryUpdateModel : IotHubModelBase
    {
        public Guid DeviceId { get; set; }
        public string DeviceType { get; set; }
        public Guid? ParentDeviceId { get; set; }
    }
}
