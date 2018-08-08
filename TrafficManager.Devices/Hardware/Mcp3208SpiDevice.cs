using System;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Adc;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Microsoft.IoT.Devices.Adc;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Devices.Hardware
{
    public class Mcp3208SpiDevice
    {
        private readonly decimal _vre;
        private readonly McpChannelByteEnum _commonChannel;
        private readonly McpChannelByteEnum _vreChannel;

	    private AdcController _controller;
	    //private readonly SpiDevice _myDevice;

        public Mcp3208SpiDevice(int spiChannel, decimal vre = 4.83m, McpChannelByteEnum commonChannel = McpChannelByteEnum.ChannelSeven, McpChannelByteEnum vreChannel = McpChannelByteEnum.ChannelEight)
        {
/*            _vre = vre;
            _commonChannel = commonChannel;
            _vreChannel = vreChannel;
            var spiSettings = new SpiConnectionSettings(0)
            {
                ClockFrequency = 500000,
                Mode = SpiMode.Mode0
            };*/

	        //var mcp = new MCP3208();
            //var spiQuery = SpiDevice.GetDeviceSelector($"SPI{spiChannel}");
           // var deviceInfo = DeviceInformation.FindAllAsync(spiQuery).AsTask().Result;
	        _controller = AdcController.GetControllersAsync(new MCP3208()).AsTask().Result.First();
			/*
            if (deviceInfo != null && deviceInfo.Count > 0)
            {
	            _myDevice = SpiDevice.FromIdAsync(deviceInfo[0].Id, spiSettings).AsTask().Result;
            }*/
        }

	    public int GetSpread(int channel)
	    {
		    var comm = _controller.OpenChannel(channel);
			
		    int maxT = 0;
		    int minT = 10000;
		    int runningTotal = 0;
			for (var j = 0; j < 5; j++)
		    {
				for (var i = 0; i < 500; i++)
				{
					var t = comm.ReadValue();
					if (t > maxT) maxT = t;
					if (t < minT) minT = t;
				}

			    runningTotal += maxT - minT;
		    }

		    return runningTotal / 5;
	    }
	    
	    /*
        public decimal GetPercentage(McpChannelByteEnum channel, int decimals = 4)
        {
            //Since I don't hae a well regulated power supply I'm going to take a reading from
            //my Vre channel and use that as the upper bound and try removing the ground rail noise
			
	        decimal reading = 0;
	        decimal avgT = 0;
	        
	        for (var i = 0; i < 500; i++)
	        {
				
		        decimal noise = Read((byte)_commonChannel);
		        decimal vreTicks = Read((byte)_vreChannel) - noise;
		        var channelTicks = Read((byte)channel) - noise;

		        reading += (channelTicks / vreTicks) * 100;
		        avgT += channelTicks;
	        }
			Debug.Write("t" + avgT / 500 + "t");
            return Math.Round(reading/500m, decimals);
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
        }*/
    }
}
