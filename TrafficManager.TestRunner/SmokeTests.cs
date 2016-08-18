using Xunit;

namespace TrafficManager.TestRunner
{
    public class SmokeTests
    {
        [Fact]
        public void TestDiscovered()
        {
            Assert.True(true);
        }

        [Fact]
        public void SadPath()
        {
            Assert.True(false);
        }
    }
}
