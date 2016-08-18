using System;

namespace TrafficManager.Domain.Models
{
    public class LogModel : IotHubModelBase
    {
        public Guid IntersectionId { get; set; }
        public bool IsError { get; set; }
        public string Message { get; set; }
    }
}