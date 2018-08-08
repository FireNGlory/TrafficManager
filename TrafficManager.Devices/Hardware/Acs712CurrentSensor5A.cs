using System;
using TrafficManager.Devices.Hardware.Base;

namespace TrafficManager.Devices.Hardware
{
	public class Acs712CurrentSensor5A : CurrentSensorBase
	{
		private readonly Mcp3208SpiDevice _mySpi;
		private readonly int _myChannel;

		public Acs712CurrentSensor5A(Guid id, Mcp3208SpiDevice mySpi, int channel) : base(id)
		{
			_mySpi = mySpi;
			_myChannel = channel;

			//the bulb should be checking the current every time it turns on.
			//If we haven't recorded a new current reading in 10 minutes we aren't working
			InOpTolerance = TimeSpan.FromMinutes(10);
			
			Tolerance = 150; 
		}

		public override void TakeReading(bool ignoreExceptions = true)
		{
			try
			{
				CurrentValue = _mySpi.GetSpread(_myChannel); //this sensor does +/- 5Amps

				LastUpdate = DateTime.UtcNow;
			}
			catch
			{
				if (!ignoreExceptions)
					throw;

				//We are probably going to ignore exceptions here. The current sensor is not a critical
				//componant and it will naturally flip to inop if we don't get a successful reading soon.
			}
		}
	}

	public class Acs712CurrentSensor30A : CurrentSensorBase
	{
		private readonly Mcp3208SpiDevice _mySpi;
		private readonly int _myChannel;

		public Acs712CurrentSensor30A(Guid id, Mcp3208SpiDevice mySpi, int channel) : base(id)
		{
			_mySpi = mySpi;
			_myChannel = channel;

			//the bulb should be checking the current every time it turns on.
			//If we haven't recorded a new current reading in 10 minutes we aren't working
			InOpTolerance = TimeSpan.FromMinutes(10);
			
			Tolerance = 50; 
		}

		public override void TakeReading(bool ignoreExceptions = true)
		{
			try
			{
				CurrentValue = _mySpi.GetSpread(_myChannel); //this sensor does +/- 5Amps

				LastUpdate = DateTime.UtcNow;
			}
			catch
			{
				if (!ignoreExceptions)
					throw;

				//We are probably going to ignore exceptions here. The current sensor is not a critical
				//componant and it will naturally flip to inop if we don't get a successful reading soon.
			}
		}
	}
}
