using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Reference;
using TrafficManager.Domain.Reference.Args;
using TrafficManager.Domain.ValueTypes;

namespace TrafficManager.Devices.Logical
{
    //Basic lamp set handles only 3-Light lamps
    public class BasicLampSet : ILampSet
    {
        public BasicLampSet(Guid id, ICollection<ILamp> lamps, int facesInDegrees, bool initRoW)
        {
            Id = id;
            Lamps = lamps;
            HasRightOfWay = initRoW;
            Facing = facesInDegrees;
        }

        public event StateChangedEvent StateChanged;

        public Guid Id { get; }
        public bool HasRightOfWay { get; private set; }
        public int Facing { get; }
        public ICollection<ILamp> Lamps { get; }

        private bool _transitioning;

        public Task<RightOfWayStateEnum> GetState()
        {
            return _transitioning 
                ? Task.FromResult(RightOfWayStateEnum.Transitioning) 
                : Task.FromResult(!HasRightOfWay 
                    ? RightOfWayStateEnum.Holding 
                    : RightOfWayStateEnum.RightOfWay);
        }

        public async Task<bool> SwitchRightOfWay()
        {
            var oldState = GetState().Result;
            var failed = false;
            _transitioning = true;

            if (HasRightOfWay)
            {
                //switching from green to red
                //do caution first
                Parallel.ForEach(Lamps, lamp =>
                {
                    if (!lamp.TransitionToState(LampStateEnum.Caution).Result) failed = true;
                });

                //HACK: This should be configurable
                await Task.Delay(TimeSpan.FromSeconds(3));

                //Then to Stop
                Parallel.ForEach(Lamps, lamp =>
                {
                    if (!lamp.TransitionToState(LampStateEnum.Stop).Result) failed = true;
                });

                //hold the right of way flag until we finish the transition
                if (!failed)
                    HasRightOfWay = false;
            }
            else
            {
                //Grab the flag before we even start the transition Error on the side of caution: assume lights are green
                HasRightOfWay = true;
                Parallel.ForEach(Lamps, lamp =>
                {
                    if (!lamp.TransitionToState(LampStateEnum.Go).Result) failed = true;
                });
            }
            _transitioning = false;

            //Check for a state change event
            var newState = GetState().Result;

            if (oldState != newState)
                OnStateChanged(oldState, newState);

            return !failed;
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

                Parallel.ForEach(Lamps, (lamp) =>
                {
                    var theSummary = lamp.GetSummary().Result;
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