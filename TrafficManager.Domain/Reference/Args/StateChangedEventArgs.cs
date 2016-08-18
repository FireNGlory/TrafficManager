using System;

namespace TrafficManager.Domain.Reference.Args
{
    public class StateChangedEventArgs : EventArgs
    {
        public Guid SourceId { get; set; }
        public int OldState { get; set; }
        public int NewState { get; set; }
        public DateTime SourceTimestamp { get; set; }
    }
}