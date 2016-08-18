using System.Threading.Tasks;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Domain.ValueTypes
{
    public interface ICurrentSensor : IDomainDevice
    {
        event StateChangedEvent StateChanged;
        
        Task<CurrentSensorStateEnum> GetState();

        void MarkInOp(bool broken);
    }
}
