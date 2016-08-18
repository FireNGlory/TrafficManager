using System;

namespace TrafficManager.Domain.Reference.Args
{
    public class CommandReceivedEventArgs : EventArgs
    {
        public string FromUser { get; set; }
        public Guid TargetId { get; set; }
        public SystemCommandEnum Command { get; set; }
        public string Arg1 { get; set; }
    }
}