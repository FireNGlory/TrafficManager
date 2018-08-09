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
            CurrentValue = 1;
        }
        public MockSensor(Guid id) : base(id)
        {
            Tolerance = .01m;
            InOpTolerance = TimeSpan.FromHours(24);
            LastUpdate = DateTime.UtcNow;
            CurrentValue = 1;
        }

	    public override void TakeReading(bool ignoreExceptions)
	    {
	    }
    }
}