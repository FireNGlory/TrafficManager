using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using TrafficManager.Devices.Hardware.Base;
using TrafficManager.Devices.Mocks;
using TrafficManager.Domain.Reference;
using TrafficManager.Domain.ValueTypes;

namespace TrafficManager.Devices.Hardware
{
    public class LedWithoutSensor : BulbBase
    {
        private readonly GpioPin _gpioPin;

        public LedWithoutSensor(Guid id, BulbTypeEnum bulbType, ICurrentSensor currentSensor, GpioPin gpioPin) : base(id, bulbType, currentSensor)
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
            
            ((MockSensor)MyCurrentSensor).SetValue(newState == BulbStateEnum.On ? 1 : 0);

            var actualState = await GetState();

            if (oldState != newState)
                OnStateChanged(oldState, actualState);

            return true;
        }
    }
}