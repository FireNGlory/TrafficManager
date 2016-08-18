using System;

namespace TrafficManager.Domain.Models
{
    public class AnomolyModel : IotHubModelBase
    {
        public Guid IntersectionId { get; set; }
        public string Function { get; set; }
        public string Description { get; set; }
        public Guid? DeviceId { get; set; }
    }
}