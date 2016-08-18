using System;

namespace TrafficManager.Domain
{
    public interface IDomainDevice
    {
        Guid Id { get; }
        string DeviceName { get; }
}
}
