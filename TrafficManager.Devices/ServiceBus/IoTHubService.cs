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
    public class IoTHubService : IEventService
    {
        public event CommandReceivedEvent CommandReceived;

        private static IoTHubService _instance;

        private readonly DeviceClient _myClient;
        private readonly ICollection<IAsyncAction> _sendTasks = new List<IAsyncAction>();
        private readonly CancellationTokenSource _tSource = new CancellationTokenSource();
        private readonly Task _listener;

        private bool _iAmPruning;

        //This should be injected
        private readonly IConfigService _cfgSvc = new InMemoryConfigService();

        private IoTHubService()
        {
            var cfg = _cfgSvc.ReadConfig().Result;

            var auth = AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(cfg.AzureIoTDeviceId,
                cfg.AzureIoTDeviceKey);

            _myClient = DeviceClient.Create(cfg.AzureIoTHubUri, auth, TransportType.Http1);

            _listener = Listener(_tSource);

        }

        public static IoTHubService Instance()
        {
            return _instance ?? (_instance = new IoTHubService());
        }

        public void UpdateDirectory(Guid deviceId, string deviceType, string deviceName, Guid? parentId)
        {
            SendMessage(EventStreamEnum.Directory, new DirectoryUpdateModel
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
            SendMessage(EventStreamEnum.StateChange, new StateChangeModel
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
            SendMessage(EventStreamEnum.Anomaly, new AnomolyModel
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

            ICollection<DeviceSummaryModel> msgList = flatList.Select(s => new DeviceSummaryModel
            {
                DeviceId = s.DeviceId,
                CurrentState = s.CurrentState.ToString(), //TODO: We need a service to get lables from enum ints.
                Timestamp = s.Timestamp,
                HasMalfunction = s.HasMalfunction
            }).ToList();

            SendMessageBatch(EventStreamEnum.Summary, msgList);


        }

        public void SendUsageUpdate(Guid deviceId, decimal factorOne, decimal factorTwo)
        {
            SendMessage(EventStreamEnum.Usage, new UsageUpdateModel
            {
                DeviceId = deviceId,
                Timestamp = DateTime.UtcNow,
                FactorOne = factorOne,
                FactorTwo = factorTwo
            });
        }

        public void SendRightOfWayChanged(Guid intersectionId, Guid oldRoWRouteId, Guid newRoWRouteId, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        public void SendLogMessage(Guid intersectionId, bool containsError, string desc, DateTime timeStamp)
        {
            SendMessage(EventStreamEnum.Log, new LogModel
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

        private void SendMessage(EventStreamEnum stream, IotHubModelBase msg)
        {
            msg.EventStream = (int)stream;

            var serialMsg = JsonConvert.SerializeObject(msg);
            var message = new Message(Encoding.UTF8.GetBytes(serialMsg));


            _sendTasks.Add(_myClient.SendEventAsync(message));
            PruneList();
        }
        private void SendMessageBatch(EventStreamEnum stream, IEnumerable<DeviceSummaryModel> msgs)
        {
            var messages = new List<Message>();
            foreach (var msg in msgs)
            {
                msg.EventStream = (int)stream;

                var serialMsg = JsonConvert.SerializeObject(msg);
                var message = new Message(Encoding.UTF8.GetBytes(serialMsg));

                messages.Add(message);
            }

            _sendTasks.Add(_myClient.SendEventBatchAsync(messages));
            PruneList();
        }

        private void PruneList()
        {
            if (_iAmPruning) return;
            
            Task.Run(() =>
            {
                _iAmPruning = true;
                var toRemove = new List<IAsyncAction>();

                foreach (var t in _sendTasks)
                {
                    if (t.AsTask().IsCompleted)
                        toRemove.Add(t);

                    //TODO: Probably should report this somewhere...
                    if (t.AsTask().IsCanceled)
                        toRemove.Add(t);

                    //TODO: Probably should report this somewhere...
                    if (t.AsTask().IsFaulted)
                        toRemove.Add(t);
                }

                foreach (var t in toRemove)
                    _sendTasks.Remove(t);
                _iAmPruning = false;
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

            Task.WaitAll(_sendTasks.Select(t => t.AsTask()).ToArray());

            _tSource.Cancel();

            _listener.Wait();

            _instance = null;
        }
    }
}
