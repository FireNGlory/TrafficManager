using System;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Domain.Models.Commands
{
    public class SystemCommandModel
    {
        public string FromUser { get; set; }
        public SystemCommandEnum RequestedCommand { get; set; }
        public Guid TargetId { get; set; }
        public string Arg1 { get; set; }
    }
}
