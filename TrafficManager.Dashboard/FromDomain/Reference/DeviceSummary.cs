using System;
using System.Collections.Generic;

namespace TrafficManager.Domain.Reference
{
    public class DeviceSummary
    {
        public DeviceSummary()
        {
            Timestamp = DateTime.UtcNow;
        }

        public Guid DeviceId { get; set; }
        public int CurrentState { get; set; }
        public bool HasMalfunction { get; set; }
        public ICollection<DeviceSummary> ChildSummaries { get; set; }
        public DateTime Timestamp { get; set; }
    }
}