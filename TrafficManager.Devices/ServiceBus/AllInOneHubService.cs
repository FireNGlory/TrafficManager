using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using TrafficManager.Devices.Configuration;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Models;
using TrafficManager.Domain.Models.Commands;
using TrafficManager.Domain.Reference;
using TrafficManager.Domain.Reference.Args;
using TrafficManager.Domain.Services;

namespace TrafficManager.Devices.ServiceBus
{
    public class AllInOneHubService : IEventService
    {
        public event CommandReceivedEvent CommandReceived;

        private static AllInOneHubService _instance;

        private readonly Task _listener;
        private readonly DeviceClient _myClient;
        private readonly List<IAsyncAction> _sendTasks = new List<IAsyncAction>();
        private readonly CancellationTokenSource _tSource = new CancellationTokenSource();
        private readonly ICollection<Message> _messageBuffer = new List<Message>();
        
        private Task _batchTimer;
        private bool _timerRunning;
        private static readonly object[] Locker = {};

        //This should be injected
        private readonly IConfigService _cfgSvc = new InMemoryConfigService();

        private AllInOneHubService()
        {
            var cfg = _cfgSvc.ReadConfig().Result;

            var auth = AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(cfg.AzureIoTDeviceId,
                cfg.AzureIoTDeviceKey);

            _myClient = DeviceClient.Create(cfg.AzureIoTHubUri, auth, TransportType.Http1);

            _listener = Listener(_tSource);
            _batchTimer = Task.Run(() =>
            {
                while (true)
                {
                    Task.Delay(TimeSpan.FromSeconds(10)).Wait();
                    _sendTasks.RemoveAll(t => t?.AsTask() == null || t.AsTask().IsCompleted);
                }
            });
        }

        public static AllInOneHubService Instance()
        {
            return _instance ?? (_instance = new AllInOneHubService());
        }

        public void UpdateDirectory(Guid deviceId, string deviceType, string deviceName, Guid? parentId)
        {
            SendMessage(EventStreamEnum.Directory, new AllInOneModel
            {
                Timestamp = DateTime.UtcNow,
                DeviceId = deviceId,
                DeviceType = deviceType,
                DeviceName = deviceName,
                ParentDeviceId = parentId
            });
        }
        
        public void SendStateChangeEvent(Guid deviceId, string deviceType, string oldState, string newState, DateTime timestamp)
        {
            SendMessage(EventStreamEnum.StateChange, new AllInOneModel
            {
                Timestamp = timestamp,
                DeviceId = deviceId,
                OldState = oldState,
                NewState = newState,
                DeviceType = deviceType
            });
        }

        public void SendAnomaly(Guid intersectionId, string function, string desc, Guid? offender, DateTime timestamp)
        {
            SendMessage(EventStreamEnum.Anomaly, new AllInOneModel
            {
                Timestamp = timestamp,
                IntersectionId = intersectionId,
                DeviceId = offender,
                Description = desc,
                Function = function
            });
        }

        public void SendSummaryUpdates(ICollection<DeviceSummary> summaries)
        {
            var flatList = summaries.ToList();
            var childList = summaries.SelectMany(s => s.ChildSummaries).ToList();

            while (childList.Any())
            {
                childList = childList.SelectMany(s => s.ChildSummaries).ToList();
                flatList.AddRange(childList);
            }

            ICollection<AllInOneModel> msgList = flatList.Select(s => new AllInOneModel
            {
                DeviceId = s.DeviceId,
                CurrentState = s.CurrentState.ToString(), //TODO: We need a service to get lables from enum ints.
                Timestamp = s.Timestamp,
                IsError = s.HasMalfunction
            }).ToList();

            SendMessageBatch(EventStreamEnum.Summary, msgList);
        }

        public void SendUsageUpdate(Guid deviceId, decimal factorOne, decimal factorTwo)
        {
            SendMessage(EventStreamEnum.Usage, new AllInOneModel
            {
                DeviceId = deviceId,
                Timestamp = DateTime.UtcNow,
                UsageFactorOne = factorOne,
                UsageFactorTwo = factorTwo
            });
        }

        public void SendRightOfWayChanged(Guid intersectionId, Guid oldRoWRouteId, Guid newRoWRouteId, DateTime timeStamp)
        {
            SendMessage(EventStreamEnum.RoWChange, new AllInOneModel
            {
                Timestamp = timeStamp,
                IntersectionId = intersectionId,
                OldState = oldRoWRouteId.ToString(),
                NewState = newRoWRouteId.ToString()
            });
        }

        public void SendLogMessage(Guid intersectionId, bool containsError, string desc, DateTime timeStamp)
        {
            SendMessage(EventStreamEnum.Log, new AllInOneModel
            {
                Timestamp = timeStamp,
                IntersectionId = intersectionId,
                IsError = containsError,
                Message = desc
            });
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void SendMessage(EventStreamEnum streamId, AllInOneModel msg)
        {
            msg.EventStream = (int)streamId;

            var serialMsg = JsonConvert.SerializeObject(new AllInOneModelDto(msg));
            var message = new Message(Encoding.UTF8.GetBytes(serialMsg));

            //_sendTasks.RemoveAll(t => t.AsTask().IsCompleted);

            _sendTasks.Add(_myClient.SendEventAsync(message));

            /*            lock(Locker)
                            _messageBuffer.Add(message);

                        StartTimer();
                        _sendTasks.RemoveAll(t => t.AsTask().IsCompleted);*/
        }

        private void SendMessageBatch(EventStreamEnum streamId, IEnumerable<AllInOneModel> msgs)
        {
            var messages = new List<Message>();
            foreach (var msg in msgs)
            {
                msg.EventStream = (int)streamId;

                var serialMsg = JsonConvert.SerializeObject(new AllInOneModelDto(msg));
                var message = new Message(Encoding.UTF8.GetBytes(serialMsg));

                messages.Add(message);
            }

            _sendTasks.Add(_myClient.SendEventBatchAsync(messages));

            //_sendTasks.RemoveAll(t => t.AsTask().IsCompleted);
        }

        private void StartTimer()
        {
            //TODO: Need better error checking here!
            if (!_timerRunning)
                _batchTimer = Task.Run(() =>
                {
                    _timerRunning = true;
                    Task.Delay(5000).Wait();
                    List<Message> thisBatch;

                    lock (Locker)
                    {
                        thisBatch = _messageBuffer.ToList();
                        _messageBuffer.Clear();
                    }

                    _sendTasks.Add(_myClient.SendEventBatchAsync(thisBatch));

                    _timerRunning = false;

                    // ReSharper disable once InconsistentlySynchronizedField
                    if (_messageBuffer.Any())
                        StartTimer();
                });
        }
        
        private async Task Listener(CancellationTokenSource token)
        {
            while (!token.IsCancellationRequested)
            {
                var receivedMessage = await _myClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                var msg = Encoding.UTF8.GetString(receivedMessage.GetBytes());

                var cmd = JsonConvert.DeserializeObject<SystemCommandModel>(msg);

                if (cmd != null) OnCommandReceived(cmd);

                await _myClient.CompleteAsync(receivedMessage);
            }
        }

        protected virtual void OnCommandReceived(SystemCommandModel theCmd)
        {
            CommandReceived?.Invoke(this, new CommandReceivedEventArgs
            {
                FromUser = theCmd.FromUser,
                Command = theCmd.RequestedCommand,
                TargetId = theCmd.TargetId,
                Arg1 = theCmd.Arg1
            });
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            _tSource.Cancel();
            _listener.Wait();
            _batchTimer.Wait();

            Task.WaitAll(_sendTasks.Select(t => t.AsTask()).ToArray());

            _instance = null;
        }
    }
}
