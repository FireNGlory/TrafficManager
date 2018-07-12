using System;
using System.Collections;
using System.Collections.Generic;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Domain.Models.Commands
{
	public class SystemCommandModel
	{
		public string Name { get; set; }
		public Guid MessageId { get; set; }
		public DateTime CreatedTime { get; set; }
		public SystemCommandParameters Parameters { get; set; }
	}

	public class SystemCommandParameters
	{
		public Guid TargetId { get; set; }
		public int? NewPreference { get; set; }
	}
    
}
