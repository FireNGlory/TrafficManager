using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Reference;
using TrafficManager.Domain.ValueTypes;

namespace TrafficManager.Domain.Aggregates
{
    public interface IIntersection : IDomainDevice, IDisposable
    {
        event RightOfWayChangedEvent RightOfWayChanged;
        event StateChangedEvent StateChanged;
        event InternalAnomalyEvent InternalAnomalyOccurred;
        
        int TransitionBaseValue { get; set; }
        ICollection<ITrafficRoute> TrafficRoutes { get; }

        void Start();

        void Stop();

        Task<Guid?> GetRightOfWayRoute();

        Task<bool> TransitionRightOfWay();
        Task<bool> UpdateRoutePreference(Guid routeId, int newMetric);
        Task<ICollection<DeviceSummary>> GetSummaries();

    }
}