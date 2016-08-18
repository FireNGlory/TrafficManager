using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Reference;
using TrafficManager.Domain.Reference.Args;
using TrafficManager.Domain.ValueTypes;

namespace TrafficManager.Devices.Logical
{
    //Basic traffic route is built to handle 2 sets of basic lamp sets
    public class BasicTrafficRoute : ITrafficRoute
    {

        public BasicTrafficRoute(Guid id, int initPreference, bool initRoW, ICollection<ILampSet> lampSets )
        {
            Id = id;
            PreferenceMetric = initPreference;
            HasRightOfWay = initRoW;
            LampSets = lampSets;
        }

        public event StateChangedEvent StateChanged;

        public Guid Id { get; }
        public int PreferenceMetric { get; private set; }
        public bool HasRightOfWay { get; private set; }
        public ICollection<ILampSet> LampSets { get; }

        private bool _transitioning;

        public Task<RightOfWayStateEnum> GetState()
        {

            return _transitioning
                ? Task.FromResult(RightOfWayStateEnum.Transitioning)
                : Task.FromResult(!HasRightOfWay
                    ? RightOfWayStateEnum.Holding
                    : RightOfWayStateEnum.RightOfWay);
        }

        public Task UpdatePreferenceMetric(int newMetric)
        {
            return Task.Run(() =>
            {
                if (newMetric < 0 || newMetric > 100)
                    throw new ArgumentOutOfRangeException(nameof(newMetric), "Metric must be between 0 and 100");

                PreferenceMetric = newMetric;
            });
        }

        public Task<bool> TransitionRightOfWay()
        {
            return Task.Run(() =>
            {
                if (_transitioning) return false;

                var oldState = GetState().Result;
                var failed = false;
                var hadRoW = HasRightOfWay;

                _transitioning = true;

                if (!HasRightOfWay) HasRightOfWay = true;

                Parallel.ForEach(LampSets, lampSet =>
                {
                    if (!lampSet.SwitchRightOfWay().Result) failed = true;
                });

                if (hadRoW && !failed)
                    HasRightOfWay = false;

                _transitioning = false;

                //Check for a state change event
                var newState = GetState().Result;

                if (oldState != newState)
                    OnStateChanged(oldState, newState);

                return !failed;
            });
        }

        protected virtual void OnStateChanged(RightOfWayStateEnum oldState, RightOfWayStateEnum newState)
        {
            StateChanged?.Invoke(this, new StateChangedEventArgs
            {
                SourceId = Id,
                OldState = (int)oldState,
                NewState = (int)newState,
                SourceTimestamp = DateTime.UtcNow
            });
        }

        public Task<DeviceSummary> GetSummary()
        {

            return Task.Run(() =>
            {
                var lampSummaries = new List<DeviceSummary>();
                var hasMalfunction = false;
                var tState = GetState();

                Parallel.ForEach(LampSets, (lampSet) =>
                {
                    var theSummary = lampSet.GetSummary().Result;
                    if (theSummary.HasMalfunction) hasMalfunction = true;

                    lampSummaries.Add(theSummary);
                });

                var ret = new DeviceSummary
                {
                    DeviceId = Id,
                    CurrentState = (int)tState.Result,
                    HasMalfunction = hasMalfunction,
                    ChildSummaries = lampSummaries
                };

                return ret;
            });
        }
    }
}