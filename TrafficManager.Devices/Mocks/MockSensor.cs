using System;
using TrafficManager.Devices.Hardware.Base;

namespace TrafficManager.Devices.Mocks
{
    public class MockSensor : CurrentSensorBase
    {
        public MockSensor() : base(Guid.NewGuid())
        {
            Tolerance = .01m;
            InOpTolerance = TimeSpan.FromHours(24);
            LastUpdate = DateTime.UtcNow;
            CurrentValue = 0;
        }
        public MockSensor(Guid id) : base(id)
        {
            Tolerance = .01m;
            InOpTolerance = TimeSpan.FromHours(24);
            LastUpdate = DateTime.UtcNow;
            CurrentValue = 0;
        }

        public void SetValue(decimal value)
        {
            var oldState = GetState().Result;

            LastUpdate = DateTime.UtcNow;
            CurrentValue = value;

            var newState = GetState().Result;

            if (oldState != newState)
                OnStateChanged(oldState, newState);
        }

	    public override void TakeReading(bool ignoreExceptions)
	    {
	    }
    }
}