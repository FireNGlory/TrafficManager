using System;

namespace TrafficManager.Domain.Models
{
    public class DeviceSummaryModel : IotHubModelBase
    {
        public Guid DeviceId { get; set; }
        public string CurrentState { get; set; }
        public bool HasMalfunction { get; set; }
    }
}