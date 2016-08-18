using TrafficManager.Devices.Mocks;
using TrafficManager.Domain.ValueTypes;

namespace TrafficManager.TestRunner
{
    public static class Builder
    {
        public static ICurrentSensor GetCurrentSensor()
        {
            return new MockSensor();
        }
    }
}