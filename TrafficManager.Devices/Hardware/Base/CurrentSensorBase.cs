using System;
using System.Threading.Tasks;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Reference;
using TrafficManager.Domain.Reference.Args;
using TrafficManager.Domain.ValueTypes;

namespace TrafficManager.Devices.Hardware.Base
{
    public abstract class CurrentSensorBase : ICurrentSensor
    {
        public event StateChangedEvent StateChanged;

        protected decimal Tolerance;
        protected decimal CurrentValue;
        protected DateTime LastUpdate;
        protected TimeSpan InOpTolerance;
        protected bool ImBroken;

        protected CurrentSensorBase(Guid id)
        {
            Id = id;
        }

	    public abstract void TakeReading(bool ignoreExceptions);

        public Guid Id { get; }

        public Task<CurrentSensorStateEnum> GetState()
        {
            return Task.Run(() =>
            {
				TakeReading(true);

                if (ImBroken || LastUpdate + InOpTolerance <= DateTime.UtcNow)
                    return CurrentSensorStateEnum.InOperable;

                return CurrentValue <= Tolerance 
                    ? CurrentSensorStateEnum.Idle 
                    : CurrentSensorStateEnum.Flowing;
            });
        }

        public void MarkInOp(bool broken)
        {
            ImBroken = broken;
        }
		
        protected virtual void OnStateChanged(CurrentSensorStateEnum oldState, CurrentSensorStateEnum newState)
        {
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