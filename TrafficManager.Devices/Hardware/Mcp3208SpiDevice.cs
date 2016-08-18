using System;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Devices.Hardware
{
    public class Mcp3208SpiDevice
    {
        private readonly decimal _vre;
        private readonly McpChannelByteEnum _commonChannel;
        private readonly McpChannelByteEnum _vreChannel;
        private readonly SpiDevice _myDevice;

        public Mcp3208SpiDevice(int spiChannel, decimal vre = 4.83m, McpChannelByteEnum commonChannel = McpChannelByteEnum.ChannelSeven, McpChannelByteEnum vreChannel = McpChannelByteEnum.ChannelEight)
        {
            _vre = vre;
            _commonChannel = commonChannel;
            _vreChannel = vreChannel;
            var spiSettings = new SpiConnectionSettings(0)
            {
                ClockFrequency = 2000000,
                Mode = SpiMode.Mode3
            };
            
            var spiQuery = SpiDevice.GetDeviceSelector($"SPI{spiChannel}");
            var deviceInfo = DeviceInformation.FindAllAsync(spiQuery).AsTask().Result;

            if (deviceInfo != null && deviceInfo.Count > 0)
            {
                _myDevice = SpiDevice.FromIdAsync(deviceInfo[0].Id, spiSettings).AsTask().Result;
            }
        }

        public decimal GetVoltage(McpChannelByteEnum channel)
        {
            //To get the voltage I'm going to get the percentage and calulate based on expected vre
            var pct = GetPercentage(channel, 10);

            //This is not as accurate as the percentage becasue I'm using the Pi to power Vre and it's 
            //not well regulated
            return _vre * (pct/100);
        }

        public decimal GetPercentage(McpChannelByteEnum channel, int decimals = 4)
        {
            //Since I don't hae a well regulated power supply I'm going to take a reading from
            //my Vre channel and use that as the upper bound and try removing the ground rail noise

            decimal noise = Read((byte)_commonChannel);
            decimal vreTicks = Read((byte)_vreChannel) - noise;
            var channelTicks = Read((byte)channel) - noise;

            var ret = (channelTicks / vreTicks) * 100;

            return Math.Round(ret, decimals);
        }

        private int Read(byte fromChannel)
        {

            var transmitBuffer = new byte[] { 1, fromChannel, 0x00 };
            var receiveBuffer = new byte[3];

            _myDevice.TransferFullDuplex(transmitBuffer, receiveBuffer);

            //first byte returned is 0 (00000000), 
            //second byte returned we are only interested in the last 4 bits 00001111 (mask of &15) 
            //then shift result 8 bits to make room for the data from the 3rd byte (makes 12 bits total)
            //third byte, need all bits, simply add it to the above result 
            return ((receiveBuffer[1] & 15) << 8) + receiveBuffer[2];
        }
    }
}
