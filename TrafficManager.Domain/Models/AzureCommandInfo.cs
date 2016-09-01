using System.Collections.Generic;

namespace TrafficManager.Domain.Models
{
    public class AzureCommandInfo
    {
        public string Name { get; set; }
        public ICollection<AzureCommandParams> Parameters { get; set; }
    }
}