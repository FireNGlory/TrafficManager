using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using TrafficManager.Devices.Hardware.Base;
using TrafficManager.Devices.Mocks;
using TrafficManager.Domain.Reference;
using TrafficManager.Domain.ValueTypes;

namespace TrafficManager.Devices.Hardware
{
    public class RealBulbWithSensor : BulbBase
    {
        private readonly GpioPin _gpioPin;

        public RealBulbWithSensor(Guid id, BulbTypeEnum bulbType, GpioPin gpioPin) : base(id, bulbType)
        {

            _gpioPin = gpioPin;
            _gpioPin.SetDriveMode(GpioPinDriveMode.Output);
            _gpioPin.Write(GpioPinValue.Low);
            Task.Delay(50).Wait();
            _gpioPin.Write(GpioPinValue.High);
            Task.Delay(50).Wait();
            _gpioPin.Write(GpioPinValue.Low);
            Task.Delay(50).Wait();
            _gpioPin.Write(GpioPinValue.High);
            Task.Delay(50).Wait();

        }

        public override async Task<bool> TransitionToState(BulbStateEnum newState)
        {
            var oldState = await GetState();
            LastStateRequest = newState;

            _gpioPin.Write(newState == BulbStateEnum.On ? GpioPinValue.Low : GpioPinValue.High);
/*
            
            //HACK: Having trouble getting reliable readings from my ACS712 without a steady power supply using 50w bulbs
            var areWeMocking = MyCurrentSensor as MockSensor;
            areWeMocking?.SetValue(newState == BulbStateEnum.On ? 1 : 0);

            var actualState = await GetState();
*/

	        if (CurrentState != BulbStateEnum.InOperable) CurrentState = newState;
            if (oldState != newState)
                OnStateChanged(oldState, newState);

            return true;
        }
    }
}
