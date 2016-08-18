using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Domain.ValueTypes
{
    public interface ILampSet : IDomainDevice
    {
        event StateChangedEvent StateChanged;
        
        bool HasRightOfWay { get; }
        int Facing { get; }
        ICollection<ILamp> Lamps { get; }

        Task<RightOfWayStateEnum> GetState();

        Task<bool> SwitchRightOfWay();

        Task<DeviceSummary> GetSummary();
    }
}