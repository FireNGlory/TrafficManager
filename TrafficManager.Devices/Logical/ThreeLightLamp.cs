using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Reference;
using TrafficManager.Domain.Reference.Args;
using TrafficManager.Domain.ValueTypes;

namespace TrafficManager.Devices.Logical
{
    public class ThreeLightLamp : ILamp
    {
        public event StateChangedEvent StateChanged;

        public Guid Id { get; }
        public ICollection<IBulb> Bulbs { get; }

        private bool _iAmTransitioning;

        public ThreeLightLamp(Guid id, ICollection<IBulb> bulbs)
        {
            Id = id;
            Bulbs = bulbs;
        }

        public Task<LampStateEnum> GetState()
        {
            return Task.Run(() =>
            {
                if (_iAmTransitioning) return LampStateEnum.Transitioning;

                var t = new[]
                {
                    GetRedLight().GetState(),
                    GetYellowLight().GetState(),
                    GetGreenLight().GetState()
                };

                Task.WaitAll(t);

                var stats = new Dictionary<string, BulbStateEnum>
                {
                    {"r" , t[0].Result},
                    {"y" , t[1].Result},
                    {"g" , t[2].Result}
                };

                //If bulbs are broke the lamp is broke
                if (stats.Any(s => s.Value == BulbStateEnum.InOperable))
                    return LampStateEnum.InOperable;

                //If a bulb is in transition the lamp is
                if (stats.Any(s => s.Value == BulbStateEnum.Transitioning))
                    return LampStateEnum.Transitioning;

                //Critical invalid state if multiple bulbs are on
                if (stats.Count(s => s.Value == BulbStateEnum.On || s.Value == BulbStateEnum.AssumedOn) != 1)
                    return LampStateEnum.CriticalMalfunction;

                //Not the job of the lamp to report the broken current sensor on a bulb. Treat assumed values as real
                if (stats["r"] == BulbStateEnum.AssumedOn || stats["r"] == BulbStateEnum.On)
                    return LampStateEnum.Stop;
                if (stats["y"] == BulbStateEnum.AssumedOn || stats["y"] == BulbStateEnum.On)
                    return LampStateEnum.Caution;
                if (stats["g"] == BulbStateEnum.AssumedOn || stats["g"] == BulbStateEnum.On)
                    return LampStateEnum.Go;

                //Unhandled state... Critical issue 
                return LampStateEnum.CriticalMalfunction;
            });
        }

        public Task<DeviceSummary> GetSummary()
        {
            return Task.Run(() =>
            {
                var tState = GetState();
                var bulbSummaries = new List<DeviceSummary>();
                var hasMalfunction = false;

                Parallel.ForEach(Bulbs, (bulb, state) =>
                {
                    var badBulb = false;
                    var theState = bulb.GetState().Result;
                    switch (theState)
                    {
                        case BulbStateEnum.AssumedOff:
                        case BulbStateEnum.AssumedOn:
                        case BulbStateEnum.InOperable:
                            hasMalfunction = true;
                            badBulb = true;
                            break;
                    }
                    bulbSummaries.Add(new DeviceSummary
                    {
                        DeviceId = bulb.Id,
                        CurrentState = (int)theState,
                        HasMalfunction = badBulb
                    });
                });

                var ret = new DeviceSummary
                {
                    DeviceId = Id,
                    HasMalfunction = hasMalfunction,
                    CurrentState = (int)tState.Result,
                    ChildSummaries = bulbSummaries
                };

                return ret;
            });
        }

        public async Task<bool> TransitionToState(LampStateEnum requestedState)
        {
            var ret = true;

            //Is this a valid request?
            if (requestedState != LampStateEnum.Go && requestedState != LampStateEnum.Caution && requestedState != LampStateEnum.Stop)
                throw new Exception("You can only request transitions to Go, Caution and Stop");

            var oldState = await GetState();

            //If it was transitioning, lets wait a little bit
            var i = 0;
            while (i++ < 10 && oldState == LampStateEnum.Transitioning)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                oldState = await GetState();
            }

            //Are we in a valid state to start a transition?
            if (oldState == LampStateEnum.CriticalMalfunction || oldState == LampStateEnum.Transitioning)
                ret = false;
            
            //We have to go through caution before we go from green to red
            if (oldState == LampStateEnum.Go && requestedState != LampStateEnum.Caution)
                ret = false;

            _iAmTransitioning = true;

            //Lat's restrict to proper transitions. InOperable is likely the bad bulb bubbling up. Try requested transition.
            if (ret && (oldState == LampStateEnum.Go || oldState == LampStateEnum.InOperable) && requestedState == LampStateEnum.Caution)
                ret = await DoTransitionToCaution();
            else if (ret && (oldState == LampStateEnum.Caution || oldState == LampStateEnum.InOperable) && requestedState == LampStateEnum.Stop)
                ret = await DoTransitionToStop();
            else if (ret && (oldState == LampStateEnum.Stop || oldState == LampStateEnum.InOperable) && requestedState == LampStateEnum.Go)
                ret = await DoTransitionToGo();
            else
                ret = false;

            //Let's clean up. finish the transition and raise state change if necessary
            _iAmTransitioning = false;

            var newState = await GetState();

            if (oldState != newState) OnStateChanged(oldState, newState);

            return ret;
        }

        protected virtual void OnStateChanged(LampStateEnum oldState, LampStateEnum newState)
        {
            StateChanged?.Invoke(this, new StateChangedEventArgs
            {
                SourceId = Id, OldState = (int) oldState, NewState = (int) newState, SourceTimestamp = DateTime.UtcNow
            });
        }

        private async Task<bool> DoTransitionToGo()
        {
            var redLight = GetRedLight();

            if (await redLight.GetState() == BulbStateEnum.InOperable)
                return await GetGreenLight().TransitionToState(BulbStateEnum.On);

            if (!await redLight.TransitionToState(BulbStateEnum.Off))
                return false;

            return await GetGreenLight().TransitionToState(BulbStateEnum.On);
        }

        private async Task<bool> DoTransitionToStop()
        {
            var yellowLight = GetYellowLight();

            if (await yellowLight.GetState() == BulbStateEnum.InOperable)
                return await GetRedLight().TransitionToState(BulbStateEnum.On);

            if (!await yellowLight.TransitionToState(BulbStateEnum.Off))
                return false;

            return await GetRedLight().TransitionToState(BulbStateEnum.On);
        }

        private async Task<bool> DoTransitionToCaution()
        {
            var greenLight = GetGreenLight();

            if (await greenLight.GetState() == BulbStateEnum.InOperable)
                return await GetYellowLight().TransitionToState(BulbStateEnum.On);

            if (!await greenLight.TransitionToState(BulbStateEnum.Off))
                return false;

            return await GetYellowLight().TransitionToState(BulbStateEnum.On);
        }

        private IBulb GetRedLight()
        {
            var ret = Bulbs.FirstOrDefault(b => b.BulbType == BulbTypeEnum.Red);

            if (ret == null)
                throw new Exception("A lamp must at least have a red, yellow and green bulb");
            return ret;
        }

        private IBulb GetGreenLight()
        {
            var ret = Bulbs.FirstOrDefault(b => b.BulbType == BulbTypeEnum.Green);

            if (ret == null)
                throw new Exception("A lamp must at least have a red, yellow and green bulb");
            return ret;
        }

        private IBulb GetYellowLight()
        {
            var ret = Bulbs.FirstOrDefault(b => b.BulbType == BulbTypeEnum.Yellow);

            if (ret == null)
                throw new Exception("A lamp must at least have a red, yellow and green bulb");
            return ret;
        }
    }
}