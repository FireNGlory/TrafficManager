using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Domain.ValueTypes
{
    public interface ILamp : IDomainDevice
    {
        event StateChangedEvent StateChanged;
        
        ICollection<IBulb> Bulbs { get; }

        Task<LampStateEnum> GetState();
        Task<DeviceSummary> GetSummary();
        Task<bool> TransitionToState(LampStateEnum requestedState);
    }
}