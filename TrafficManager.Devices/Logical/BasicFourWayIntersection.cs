using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrafficManager.Domain.Aggregates;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Reference;
using TrafficManager.Domain.Reference.Args;
using TrafficManager.Domain.ValueTypes;

namespace TrafficManager.Devices.Logical
{
    public class BasicFourWayIntersection : IIntersection {
        public event RightOfWayChangedEvent RightOfWayChanged;
        public event StateChangedEvent StateChanged;
        public event InternalAnomalyEvent InternalAnomalyOccurred;

        public BasicFourWayIntersection(Guid id, int transBase, ICollection<ITrafficRoute> routes)
        {
            Id = id;
            TransitionBaseValue = transBase;
            TrafficRoutes = routes;
            _running = false;
            _stopRequested = false;
        }
        
        public Guid Id { get; }
        public int TransitionBaseValue { get; set; }
        public ICollection<ITrafficRoute> TrafficRoutes { get; }


        private bool _running;
        private bool _stopRequested;

        private Task _sequencer;

        public void Start()
        {
            _stopRequested = false;

            _sequencer = Task.Run(() =>
            {
                _running = true;
                OnStateChanged(IntersectionStateEnum.Offline, IntersectionStateEnum.Online);
                while (!_stopRequested)
                {
                    var success = TransitionRightOfWay().Result;

                    if (!success)
                    {
                        var sumList = GetSummaries().Result;
                        OnInternalAnomalyOccurred("Sequencer", "Issue arose transitioning intersection", null,
                            sumList);
                    }
                    var row = GetRightOfWayRouteObject().Result;

                    Task.Delay((TransitionBaseValue + row.PreferenceMetric)*1000).Wait();
                }
                OnStateChanged(IntersectionStateEnum.Online, IntersectionStateEnum.Offline);
                _running = false;
            });
        }

        public void Stop()
        {
            _stopRequested = true;
        }

        public async Task<Guid?> GetRightOfWayRoute()
        {
            var row = await GetRightOfWayRouteObject();

            return row?.Id;
        }

        public async Task<bool> TransitionRightOfWay()
        {
            var row = await DoTransition();

            return row != null;
        }

        public async Task<bool> UpdateRoutePreference(Guid routeId, int newMetric)
        {
            var route = TrafficRoutes.FirstOrDefault(r => r.Id == routeId);

            if (route == null)
            {
                var sumList = await GetSummaries();
                OnInternalAnomalyOccurred("UpdateRoutePreference", $"Route not found: {routeId}", null,
                    sumList);
                return false;
            }

            await route.UpdatePreferenceMetric(newMetric);

            return true;
        }

        public Task<ICollection<DeviceSummary>> GetSummaries()
        {
            return Task.Run(() =>
            {
                var retList = new List<DeviceSummary>();

                Parallel.ForEach(TrafficRoutes, route => retList.Add(route.GetSummary().Result));

                return (ICollection<DeviceSummary>) retList;
            });
        }

        private async Task<ITrafficRoute> DoTransition()
        {
            var currRoW = await GetRightOfWayRouteObject();
            var nextRoW = TrafficRoutes.FirstOrDefault(r => r.HasRightOfWay == false);
            var success = await currRoW.TransitionRightOfWay();

            if (success)
                success = await nextRoW.TransitionRightOfWay();

            if (!success) return null;

            OnRightOfWayChanged(currRoW.Id, nextRoW.Id);

            return nextRoW;
        }

        private async Task<ITrafficRoute> GetRightOfWayRouteObject()
        {
            var row = TrafficRoutes.Where(r => r.HasRightOfWay).ToList();

            if (row.Count != 1)
            {
                var sumList = await GetSummaries();
                OnInternalAnomalyOccurred("GetRightOfWayRoute", $"Expecting 1 route but found {row.Count}", null,
                    sumList);
            }

            return row.FirstOrDefault();
        }

        protected virtual void OnStateChanged(IntersectionStateEnum oldState, IntersectionStateEnum newState)
        {
            StateChanged?.Invoke(this, new StateChangedEventArgs
            {
                SourceId = Id,
                OldState = (int)oldState,
                NewState = (int)newState,
                SourceTimestamp = DateTime.UtcNow
            });
        }

        protected virtual void OnRightOfWayChanged(Guid oldRoW, Guid newRoW)
        {
            RightOfWayChanged?.Invoke(this, new RightOfWayChangedEventArgs
            {
                IntersectionId = Id,
                OldRouteId = oldRoW,
                NewRouteId = newRoW,
                Timestamp = DateTime.UtcNow
            });
        }

        protected virtual void OnInternalAnomalyOccurred(string funcName, string desc, DeviceSummary suspectDeviceSummary, ICollection<DeviceSummary> otherDeviceSummaries)
        {
            InternalAnomalyOccurred?.Invoke(this, new InternalAnomalyEventArgs
            {
                IntersectionId = Id,
                PerformingFunction = funcName,
                Description = desc,
                SuspectDeviceSummary = suspectDeviceSummary,
                OtherDeviceSummaries = otherDeviceSummaries,
                Timestamp = DateTime.UtcNow
            });
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            Stop();
            while (_running)
                Task.Delay(100).Wait();

            _sequencer = null;
        }
    }
}