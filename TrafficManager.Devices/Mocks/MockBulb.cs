using System;
using System.Threading.Tasks;
using TrafficManager.Devices.Hardware.Base;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Devices.Mocks
{
    public class MockBulb : BulbBase
    {
        public MockBulb(BulbTypeEnum bulbType) : base(Guid.NewGuid(), bulbType, new MockSensor())
        {
        }

        public override async Task<bool> TransitionToState(BulbStateEnum newState)
        {
            var oldState = await GetState();
            LastStateRequest = newState;
            ((MockSensor)MyCurrentSensor).SetValue(newState == BulbStateEnum.On ? 1 : 0);

            var actualState = await GetState();

            if (oldState != newState)
                OnStateChanged(oldState, actualState);

            return true;

        }

    }
}