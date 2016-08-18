using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Reference;
using TrafficManager.Domain.Reference.Args;
using TrafficManager.Domain.ValueTypes;

namespace TrafficManager.Devices.Hardware.Base
{
    public class BulbBase : IBulb
    {
        public event StateChangedEvent StateChanged;
        public event BulbCycledEvent BulbCycled;

        protected bool ImBroken;
        protected BulbStateEnum LastStateRequest;
        private readonly Stopwatch _usageTimer;

        public BulbBase(Guid id, BulbTypeEnum bulbType, ICurrentSensor currentSensor)
        {
            Id = id;
            BulbType = bulbType;
            MyCurrentSensor = currentSensor;
            _usageTimer = new Stopwatch();
        }

        public Guid Id { get; }
        public BulbTypeEnum BulbType { get; }
        public ICurrentSensor MyCurrentSensor { get; }

        public async Task<BulbStateEnum> GetState()
        {
            if (ImBroken) return BulbStateEnum.InOperable;

            var sensorState = await MyCurrentSensor.GetState();

            switch (sensorState)
            {
                case CurrentSensorStateEnum.InOperable:
                    return LastStateRequest == BulbStateEnum.Off ? BulbStateEnum.AssumedOff : BulbStateEnum.AssumedOn;
                case CurrentSensorStateEnum.Idle:
                    return BulbStateEnum.Off;
                case CurrentSensorStateEnum.Flowing:
                    return BulbStateEnum.On;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual Task<bool> TransitionToState(BulbStateEnum newState)
        {
            throw new NotImplementedException();
        }

        public void MarkInOp(bool broken)
        {
            ImBroken = broken;
        }

        protected virtual void OnStateChanged(BulbStateEnum oldState, BulbStateEnum newState)
        {
            //start the usage stopwatch to report the stats for predictive maintenance
            //Starting when we are going from off to on
            if (oldState != BulbStateEnum.On && oldState != BulbStateEnum.AssumedOn &&
                (newState == BulbStateEnum.On || newState == BulbStateEnum.AssumedOn))
                _usageTimer.Start();

            //if we are going from on to off itis time to report the usage cycle
            if (oldState == BulbStateEnum.On || oldState == BulbStateEnum.AssumedOn 
                && _usageTimer.IsRunning)
            {
                _usageTimer.Stop();

                BulbCycled?.Invoke(this, new BulbCycledEventArgs
                {
                    BulbId = Id,
                    SecondsOn = _usageTimer.Elapsed.TotalSeconds
                });
                _usageTimer.Reset();
            }

            StateChanged?.Invoke(this, new StateChangedEventArgs
            {
                SourceId = Id,
                OldState = (int)oldState,
                NewState = (int)newState,
                SourceTimestamp = DateTime.UtcNow
            });
        }

    }
}