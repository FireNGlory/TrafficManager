using System;
using System.Collections.Generic;

namespace TrafficManager.Domain.Reference.Args
{
    public class CommandReceivedEventArgs : EventArgs
    {
        public SystemCommandEnum Command { get; set; }
        public ICollection<KeyValuePair<string, object>> Parameters { get; set; }
    }
}