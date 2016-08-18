using System;
using TrafficManager.Devices.Hardware.Base;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Devices.Hardware
{
    public class Acs712CurrentSensor5A : CurrentSensorBase
    {
        private readonly Mcp3208SpiDevice _mySpi;
        private readonly McpChannelByteEnum _myChannel;

        public Acs712CurrentSensor5A(Guid id, Mcp3208SpiDevice mySpi, McpChannelByteEnum channel) : base(id)
        {
            _mySpi = mySpi;
            _myChannel = channel;

            //the bulb should be checking the current every time it turns on.
            //If we haven't recorded a new current reading in 10 minutes we aren't working
            InOpTolerance = TimeSpan.FromMinutes(10);

            //My 50 watt bulb should draw just under half an amp. I'll say it's on if we are drawing over .2
            Tolerance = 0.2m; 

            TakeReading();
        }

        public void TakeReading(bool ignoreExceptions = true)
        {
            try
            {
                var pct = _mySpi.GetPercentage(_myChannel);
                
                //50% should be no current
                pct = pct - 50;

                CurrentValue = Math.Abs(5 * (pct/100)); //this sensor does +/- 5Amps

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
