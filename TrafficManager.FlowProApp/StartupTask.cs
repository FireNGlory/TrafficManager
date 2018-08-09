using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using TrafficManager.Devices.Configuration;
using TrafficManager.Devices.Hardware;
using TrafficManager.Devices.Logical;
using TrafficManager.Devices.Mocks;
using TrafficManager.Devices.ServiceBus;
using TrafficManager.Domain.Aggregates;
using TrafficManager.Domain.Reference;
using TrafficManager.Domain.Reference.Args;
using TrafficManager.Domain.Services;
using TrafficManager.Domain.ValueTypes;

namespace TrafficManager.FlowProApp
{
    public sealed class StartupTask : IBackgroundTask
    {
        private readonly IConfigService _cfgSvc = new InMemoryConfigService();

        //Bulbs for north lamp (real bulbs)
        private readonly ICollection<IBulb> _nBulbs = new List<IBulb>();

        //Bulbs for south lamp
        private readonly ICollection<IBulb> _sBulbs = new List<IBulb>();

        //Bulbs for east lamp
        private readonly ICollection<IBulb> _eBulbs = new List<IBulb>();

        //Bulbs for west lamp
        private readonly ICollection<IBulb> _wBulbs = new List<IBulb>();

        private ILamp _nLamp;
        private ILamp _sLamp;
        private ILamp _eLamp;
        private ILamp _wLamp;

        private ILampSet _nSet;
        private ILampSet _sSet;
        private ILampSet _eSet;
        private ILampSet _wSet;

        private ITrafficRoute _nsRoute;
        private ITrafficRoute _ewRoute;

        private IIntersection _theIntersection;

        private IEventService _eventor;
        private CancellationTokenSource _tSource;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _tSource = new CancellationTokenSource();

            var deferral = taskInstance.GetDeferral();

            _eventor = AllInOneHubService.Instance();

            var cfg = await _cfgSvc.ReadConfig();

            _eventor.SendOnline();

            InitGpio(cfg).Wait(_tSource.Token);

	       // var t = Reporter();

            BindEvents();

            _eventor.CommandReceived += EventorOnCommandReceived;
            _theIntersection.Start();
            
            while (!_tSource.Token.IsCancellationRequested)
                await Task.Delay(TimeSpan.FromHours(1), _tSource.Token);

            _eventor.Dispose();

            _theIntersection.Stop();

            _theIntersection.Dispose();

            _eventor.Dispose();

            deferral.Complete();
        }

	    private Task Reporter()
	    {
		    return Task.Run(() =>
		    {
			    while (true)
			    {
					Debug.Write($"{Mcp3208SpiDevice.Instance().GetSpread(0)}-{Mcp3208SpiDevice.Instance().GetSpread(1)}-{Mcp3208SpiDevice.Instance().GetSpread(2)}-{Mcp3208SpiDevice.Instance().GetSpread(3)}\n");
					Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
			    }
		    });
	    }
        private void EventorOnCommandReceived(object sender, CommandReceivedEventArgs args)
        {
            Guid? targetId = null;

            if (args.Parameters.Any(p => p.Key == "targetId"))
                targetId = new Guid(args.Parameters.First(p => p.Key == "targetId").Value.ToString());

            _eventor.SendLogMessage(_theIntersection.Id, false, 
                $"Received {args.Command} command  for {targetId}", DateTime.UtcNow);
			

	        var workBulb = _nBulbs.FirstOrDefault(b => b.Id == targetId)
	                       ?? _sBulbs.FirstOrDefault(b => b.Id == targetId)
	                       ?? _eBulbs.FirstOrDefault(b => b.Id == targetId)
	                       ?? _wBulbs.FirstOrDefault(b => b.Id == targetId);

            ITrafficRoute workRoute;
            switch (args.Command)
            {
                case SystemCommandEnum.None:
                    break;
                case SystemCommandEnum.BringOnline:
                    _theIntersection.Start();
                    break;
                case SystemCommandEnum.RequestStatus:

                    var lamp = _nSet.Lamps.FirstOrDefault(l => l.Id == targetId)
                                ?? _sSet.Lamps.FirstOrDefault(l => l.Id == targetId)
                                ?? _eSet.Lamps.FirstOrDefault(l => l.Id == targetId)
                                ?? _wSet.Lamps.FirstOrDefault(l => l.Id == targetId);

                    if (lamp != null)
                    {
                        lamp.GetSummary().ContinueWith(r =>
                        _eventor.SendSummaryUpdates(new List<DeviceSummary> { r.Result})
                        ).Wait();
                        return;
                    }

                    var workSet = _nsRoute.LampSets.FirstOrDefault(s => s.Id == targetId)
                                ?? _ewRoute.LampSets.FirstOrDefault(s => s.Id == targetId);

                    if (workSet != null)
                    {
                        workSet.GetSummary().ContinueWith(r =>
                        _eventor.SendSummaryUpdates(new List<DeviceSummary> { r.Result })
                        ).Wait(); 
                        return;
                    }

                    workRoute = _theIntersection.TrafficRoutes.FirstOrDefault(r => r.Id == targetId);

                    if (workRoute != null)
                    {
                        workRoute.GetSummary().ContinueWith(r =>
                        _eventor.SendSummaryUpdates(new List<DeviceSummary> { r.Result })
                        ).Wait(); 
                        return;
                    }

                    if (_theIntersection.Id == targetId)
                        _theIntersection.GetSummaries().ContinueWith(r =>
                        _eventor.SendSummaryUpdates(r.Result)
                        ).Wait(); 

                    break;
                case SystemCommandEnum.UpdateRoutePreference:
                    workRoute = _theIntersection.TrafficRoutes.FirstOrDefault(r => r.Id == targetId);
                    if (workRoute == null) return;

                    int? newPreference;

                    if (args.Parameters.Any(p => p.Key == "preference"))
                        newPreference = (int) args.Parameters.First(p => p.Key == "preference").Value;
                    else
                        return;

                    workRoute.UpdatePreferenceMetric(newPreference.Value);
                    break;
                case SystemCommandEnum.ReplaceBulb:
                    if (workBulb == null) return;
                    workBulb.MarkInOp(false);
                    break;
                case SystemCommandEnum.ReplaceSensor:
                    if (workBulb == null) return;
                    //workBulb.MyCurrentSensor.MarkInOp(false);
                    break;
                case SystemCommandEnum.SimulateBulbFailure:
                    if (workBulb == null) return;
                    workBulb.MarkInOp(true);
                    break;
                case SystemCommandEnum.SimulateSensorFailure:
                    if (workBulb == null) return;
                    //workBulb.MyCurrentSensor.MarkInOp(true);
                    break;
                case SystemCommandEnum.TakeOffline:
                    _theIntersection.Stop();
                    break;
                case SystemCommandEnum.Shutdown:
                    _tSource.Cancel(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task InitGpio(ConfigurationSettings cfg)
        {
            try
            {

                var gpio = GpioController.GetDefault();

                // Show an error if there is no GPIO controller
                if (gpio == null)
                {
                    _eventor.SendLogMessage(cfg.IntersectionId, true, "There is no GPIO controller on this device.", DateTime.UtcNow);
                    return;
                }

                //Construct the bulbs
                await BuildBulbs(gpio, cfg);

                //Construct the Lamps
                await BuildLamps(cfg);

                //Put them in sets (only one lamp sets for this case...)
                _nSet = new BasicLampSet(cfg.NorthLampSetId, new List<ILamp> { _nLamp }, 0, false);
                _sSet = new BasicLampSet(cfg.SouthLampSetId, new List<ILamp> { _sLamp }, 180, false);
                _eSet = new BasicLampSet(cfg.EastLampSetId, new List<ILamp> { _eLamp }, 90, true);
                _wSet = new BasicLampSet(cfg.WestLampSetId, new List<ILamp> { _wLamp }, 270, true);

                //Establish the lamp sets on traffic routes
                _nsRoute = new BasicTrafficRoute(cfg.NorthSouthRouteId, 5, false, new List<ILampSet> { _nSet, _sSet });

                _nsRoute.TransitionRightOfWay().Wait();
                await Task.Delay(1000);
                _nsRoute.TransitionRightOfWay().Wait();
                await Task.Delay(1000);


                _ewRoute = new BasicTrafficRoute(cfg.EastWestRouteId, 0, true, new List<ILampSet> { _eSet, _wSet });

                _ewRoute.TransitionRightOfWay().Wait();
                await Task.Delay(1000);
                _ewRoute.TransitionRightOfWay().Wait();
                await Task.Delay(1000);


                //Put the intersection together
                _theIntersection = new BasicFourWayIntersection(cfg.IntersectionId, 3, new List<ITrafficRoute>
                {
                    _nsRoute, _ewRoute
                });

                _eventor.SendLogMessage(cfg.IntersectionId, false, "POST completed.", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _eventor.SendLogMessage(cfg.IntersectionId, true, ex.ToString(), DateTime.UtcNow);
                throw;
            }
        }

        private async Task BuildBulbs(GpioController gpio, ConfigurationSettings cfg)
        {
            await Task.Run(() =>
            {
                _nBulbs.Add(new RealBulbWithSensor(cfg.NorthRedBulbId, BulbTypeEnum.Red, 
                    gpio.OpenPin(cfg.NorthRedBulbPinId)));
                _nBulbs.Add(new RealBulbWithSensor(cfg.NorthYellowBulbId, BulbTypeEnum.Yellow, 
                    gpio.OpenPin(cfg.NorthYellowBulbPinId)));
                _nBulbs.Add(new RealBulbWithSensor(cfg.NorthGreenBulbId, BulbTypeEnum.Green, 
                    gpio.OpenPin(cfg.NorthGreenBulbPinId)));


                //The rest of the lamps are simulated on the breadboard with LEDs
                _sBulbs.Add(new RealBulbWithSensor(cfg.SouthRedBulbId, BulbTypeEnum.Red, 
                    gpio.OpenPin(cfg.SouthRedBulbPinId)));
                _sBulbs.Add(new RealBulbWithSensor(cfg.SouthYellowBulbId, BulbTypeEnum.Yellow, 
                    gpio.OpenPin(cfg.SouthYellowBulbPinId)));
                _sBulbs.Add(new RealBulbWithSensor(cfg.SouthGreenBulbId, BulbTypeEnum.Green, 
                    gpio.OpenPin(cfg.SouthGreenBulbPinId)));

                _eBulbs.Add(new RealBulbWithSensor(cfg.EastRedBulbId, BulbTypeEnum.Red, 
                    gpio.OpenPin(cfg.EastRedBulbPinId)));
                _eBulbs.Add(new RealBulbWithSensor(cfg.EastYellowBulbId, BulbTypeEnum.Yellow, 
                    gpio.OpenPin(cfg.EastYellowBulbPinId)));
                _eBulbs.Add(new RealBulbWithSensor(cfg.EastGreenBulbId, BulbTypeEnum.Green, 
                    gpio.OpenPin(cfg.EastGreenBulbPinId)));

                _wBulbs.Add(new RealBulbWithSensor(cfg.WestRedBulbId, BulbTypeEnum.Red, 
                    gpio.OpenPin(cfg.WestRedBulbPinId)));
                _wBulbs.Add(new RealBulbWithSensor(cfg.WestYellowBulbId, BulbTypeEnum.Yellow, 
                    gpio.OpenPin(cfg.WestYellowBulbPinId)));
                _wBulbs.Add(new RealBulbWithSensor(cfg.WestGreenBulbId, BulbTypeEnum.Green, 
                    gpio.OpenPin(cfg.WestGreenBulbPinId)));

                var t = new[]
                {
                    _nBulbs.First(b => b.BulbType == BulbTypeEnum.Red).TransitionToState(BulbStateEnum.On), _sBulbs.First(b => b.BulbType == BulbTypeEnum.Red).TransitionToState(BulbStateEnum.On), _wBulbs.First(b => b.BulbType == BulbTypeEnum.Green).TransitionToState(BulbStateEnum.On), _eBulbs.First(b => b.BulbType == BulbTypeEnum.Green).TransitionToState(BulbStateEnum.On)
                };

                Task.WaitAll(t);
            });
        }

        private async Task BuildLamps(ConfigurationSettings cfg)
        {
            //My *north* lamp is the real bulb breakout 
            _nLamp = new ThreeLightLamp(cfg.NorthLampId, _nBulbs,
	            //new Acs712CurrentSensor5A(cfg.NorthRedBulbSensorId, 0)
				new MockSensor()
		        );

            _nLamp.TransitionToState(LampStateEnum.Go).Wait();
            await Task.Delay(200);
            _nLamp.TransitionToState(LampStateEnum.Caution).Wait();
            await Task.Delay(200);
            _nLamp.TransitionToState(LampStateEnum.Stop).Wait();
            await Task.Delay(200);

            //The rest of the lamps are simulated on the breadboard with LEDs
            _sLamp = new ThreeLightLamp(cfg.SouthLampId, _sBulbs,
	            //new Acs712CurrentSensor30A(cfg.NorthRedBulbSensorId, 2)
				new MockSensor()
		        );

            _sLamp.TransitionToState(LampStateEnum.Go).Wait();
            await Task.Delay(200);
            _sLamp.TransitionToState(LampStateEnum.Caution).Wait();
            await Task.Delay(200);
            _sLamp.TransitionToState(LampStateEnum.Stop).Wait();
            await Task.Delay(200);

            _eLamp = new ThreeLightLamp(cfg.EastLampId, _eBulbs,
	            //new Acs712CurrentSensor5A(cfg.NorthRedBulbSensorId, 3)
	            new MockSensor()
	            );

            _eLamp.TransitionToState(LampStateEnum.Caution).Wait();
            await Task.Delay(200);
            _eLamp.TransitionToState(LampStateEnum.Stop).Wait();
            await Task.Delay(200);
            _eLamp.TransitionToState(LampStateEnum.Go).Wait();
            await Task.Delay(200);

            _wLamp = new ThreeLightLamp(cfg.WestLampId, _wBulbs,
	           // new Acs712CurrentSensor30A(cfg.NorthRedBulbSensorId, 1)
				new MockSensor()
		        );

            _wLamp.TransitionToState(LampStateEnum.Caution).Wait();
            await Task.Delay(200);
            _wLamp.TransitionToState(LampStateEnum.Stop).Wait();
            await Task.Delay(200);
            _wLamp.TransitionToState(LampStateEnum.Go).Wait();
        }

        //This is where events bubble up from the underlying hardware on their way through the _eventor 
        //interface where we will be using our AzureIoT implementation to push them on to the serviec bus
        private void BindEvents()
        {
            var routes = _theIntersection.TrafficRoutes.ToList();
            var sets = routes.SelectMany(r => r.LampSets).ToList();
            var lamps = sets.SelectMany(ls => ls.Lamps).ToList();
            var bulbs = lamps.SelectMany(l => l.Bulbs).ToList();
            var sensors = lamps.Select(b => b.CurrentSensor).ToList();


            sensors.ForEach(s => s.StateChanged += (sender, args) =>
            {
                //Only report inop
                if (args.NewState != 9999 && args.OldState != 9999) return;

                var oldS = (CurrentSensorStateEnum)args.OldState;
                var newS = (CurrentSensorStateEnum)args.NewState;

                _eventor.SendStateChangeEvent(args.SourceId, "Sensor", oldS.ToString(), newS.ToString(), args.SourceTimestamp);
            });

            bulbs.ForEach(s => s.StateChanged += (sender, args) =>
            {
                var oldS = (BulbStateEnum)args.OldState;
                var newS = (BulbStateEnum)args.NewState;

                _eventor.SendStateChangeEvent(args.SourceId, "Bulb", oldS.ToString(), newS.ToString(), args.SourceTimestamp);
            });
            
            //For bulb usage factor one is the time it was on and factor 2 is the cycle count.
            //Considering that turning a bulb on is hard on the materials it shoudld factor in to MTBF
            bulbs.ForEach(s => s.BulbCycled += (sender, args) => 
                _eventor.SendUsageUpdate(args.BulbId, Convert.ToDecimal(args.SecondsOn), 1));

            lamps.ForEach(s => s.StateChanged += (sender, args) =>
            {
                var oldS = (LampStateEnum)args.OldState;
                var newS = (LampStateEnum)args.NewState;

                _eventor.SendStateChangeEvent(args.SourceId, "Lamp", oldS.ToString(), newS.ToString(), args.SourceTimestamp);
            });

            sets.ForEach(s => s.StateChanged += (sender, args) =>
            {
                var oldS = (RightOfWayStateEnum)args.OldState;
                var newS = (RightOfWayStateEnum)args.NewState;

                _eventor.SendStateChangeEvent(args.SourceId, "LampSet", oldS.ToString(), newS.ToString(), args.SourceTimestamp);
            });

            routes.ForEach(s => s.StateChanged += (sender, args) =>
            {
                var oldS = (RightOfWayStateEnum)args.OldState;
                var newS = (RightOfWayStateEnum)args.NewState;

                _eventor.SendStateChangeEvent(args.SourceId, "Route", oldS.ToString(), newS.ToString(), args.SourceTimestamp);
            });

            _theIntersection.StateChanged += (sender, args) =>
            {
                var oldS = (IntersectionStateEnum)args.OldState;
                var newS = (IntersectionStateEnum)args.NewState;

                _eventor.SendStateChangeEvent(args.SourceId, "Intersection", oldS.ToString(), newS.ToString(), args.SourceTimestamp);
            };

            _theIntersection.InternalAnomalyOccurred += (sender, args) =>
                _eventor.SendAnomaly(args.IntersectionId, args.PerformingFunction, args.Description, args.SuspectDeviceSummary?.DeviceId, args.Timestamp);

            _theIntersection.RightOfWayChanged += (sender, args) =>
                _eventor.SendRightOfWayChanged(args.IntersectionId, args.OldRouteId, args.NewRouteId, args.Timestamp);

        }
    }
}
