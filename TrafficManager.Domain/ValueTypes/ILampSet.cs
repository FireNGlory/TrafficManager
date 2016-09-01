using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Domain.ValueTypes
{
    public interface ILampSet : IDomainDevice
    {
        bool HasRightOfWay { get; }
        int Facing { get; }
        ICollection<ILamp> Lamps { get; }

        Task<RightOfWayStateEnum> GetState();

        Task<bool> SwitchRightOfWay();

        Task<DeviceSummary> GetSummary();
    }
}