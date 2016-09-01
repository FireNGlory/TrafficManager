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
        public ICollection<KeyValuePair<string, object>>  Parameters { get; set; }
    }
    
}
