using System;

namespace TrafficManager.Domain.Models
{
    public abstract class IotHubModelBase
    {
        public int EventStream { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
