using System;

namespace TrafficManager.Domain.Reference.Args
{
    public class BulbCycledEventArgs : EventArgs
    {
        public Guid BulbId { get; set; }
        public double SecondsOn { get; set; }
    }
}