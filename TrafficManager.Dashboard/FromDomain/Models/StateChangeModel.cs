using System;

namespace TrafficManager.Domain.Models
{
    public class StateChangeModel : IotHubModelBase
    {
        public Guid DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string OldState { get; set; }
        public string NewState { get; set; }
    }
}