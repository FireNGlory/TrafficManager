using System;

namespace TrafficManager.Domain.Reference.Args
{
    public class RightOfWayChangedEventArgs : EventArgs
    {
        public Guid IntersectionId { get; set; }
        public Guid OldRouteId { get; set; }
        public Guid NewRouteId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
