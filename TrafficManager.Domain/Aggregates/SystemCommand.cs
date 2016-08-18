using System;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Domain.Aggregates
{
    public class SystemCommand
    {
        public SystemCommandEnum RequestedCommand { get; set; }
        public Guid TargetId { get; set; }
    }
}
