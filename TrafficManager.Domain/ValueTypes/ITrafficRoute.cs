using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Domain.ValueTypes
{
    public interface ITrafficRoute : IDomainDevice
    {
        event StateChangedEvent StateChanged;
        
        int PreferenceMetric { get; }
        bool HasRightOfWay { get; }
        ICollection<ILampSet> LampSets { get; }

        Task<RightOfWayStateEnum> GetState();
        Task<DeviceSummary> GetSummary();

        Task UpdatePreferenceMetric(int newMetric);

        Task<bool> TransitionRightOfWay();
    }
}