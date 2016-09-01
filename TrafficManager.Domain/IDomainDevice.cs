using System;
using TrafficManager.Domain.EventHandlers;

namespace TrafficManager.Domain
{
    public interface IDomainDevice
    {
        event StateChangedEvent StateChanged;

        Guid Id { get; }
    }
}
