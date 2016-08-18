using System;

namespace TrafficManager.Domain.Models
{
    public class UsageUpdateModel : IotHubModelBase
    {
        public Guid DeviceId { get; set; }
        public decimal FactorOne { get; set; }
        public decimal FactorTwo { get; set; }
    }
}