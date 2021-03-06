﻿using System;

namespace TrafficManager.Domain.Models
{
    public abstract class IotHubModelBaseDto
    {
        public string DeviceId { get; set; }
        public int sn { get; set; }
        public DateTime ts { get; set; }
        public abstract IotHubModelBase ToFullModel();
    }
}