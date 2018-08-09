using System;

namespace TrafficManager.Domain.Models.Commands
{
    public class SystemCommandModel
    {
        public string Name { get; set; }
        public Guid MessageId { get; set; }
        public DateTime CreatedTime { get; set; }
	    public Guid TargetId { get; set; }
	    public int? NewPreference { get; set; }
    }
}
