using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Adc;
using Microsoft.IoT.Devices.Adc;

namespace TrafficManager.Devices.Hardware
{
    public class Mcp3208SpiDevice
    {
	    private CancellationTokenSource _tSource;
	    private static Mcp3208SpiDevice _instance;
	    private readonly AdcController _controller;
		private int[] _vals = new int[4];
	    //private readonly SpiDevice _myDevice;

        private Mcp3208SpiDevice()
        {
	        _controller = AdcController.GetControllersAsync(new MCP3208()).AsTask().Result.First();
			_tSource = new CancellationTokenSource();
	        ReadVals(_tSource.Token);
        }

	    public static Mcp3208SpiDevice Instance()
	    {
		    return _instance ?? (_instance = new Mcp3208SpiDevice());
	    }

	    public int GetSpread(int channel)
	    {
		    return _vals[channel];
	    }
	    private void ReadVals(CancellationToken token)
	    {
		    Task.Run(() =>
		    {
			    if (token.IsCancellationRequested) return;

			    for (var c = 0; c <= 3; c++)
			    {
				    var comm = _controller.OpenChannel(c);

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

				    _vals[c] = runningTotal / 5;
			    }
		    });
	    }


    }
}
