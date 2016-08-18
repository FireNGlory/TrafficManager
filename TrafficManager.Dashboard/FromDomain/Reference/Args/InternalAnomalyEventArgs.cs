using System;
using System.Collections.Generic;

namespace TrafficManager.Domain.Reference.Args
{
    public class InternalAnomalyEventArgs : EventArgs
    {
        public Guid IntersectionId { get; set; }
        public string Description { get; set; }
        public string PerformingFunction { get; set; }
        public DeviceSummary SuspectDeviceSummary { get; set; }
        public ICollection<DeviceSummary> OtherDeviceSummaries { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
