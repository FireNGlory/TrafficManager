using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using TrafficManager.Domain.Models;
using TrafficManager.Domain.Models.Commands;

namespace TrafficManager.Dashboard.Hubs
{
    public class Transporter : IDisposable
    {
        private static Transporter _instance;

        private readonly CancellationTokenSource _tokenSrc = new CancellationTokenSource();
        private readonly ServiceClient _serviceClt;
        private readonly List<Task> _sbTasks = new List<Task>();

        private Transporter()
        {
            const string connectionString = "HostName=PieceOfPiHub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=jIUi1GLea8dDnwSu1j5N5fM/aJN7E4ubKxoRxUgUbGo=";
            const string iotHubToClientEndpoint = "messages/events";

            _serviceClt = ServiceClient.CreateFromConnectionString(connectionString);

            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubToClientEndpoint);

            foreach (var partition in eventHubClient.GetRuntimeInformation().PartitionIds)
            {
                var receiver = eventHubClient
                    .GetDefaultConsumerGroup()
                    .CreateReceiver(partition, DateTime.Now);
                _sbTasks.Add(Listen(receiver));
            }
        }
        public static Transporter Instance()
        {
            return _instance ?? (_instance = new Transporter());
        }

        public void SendCommand(string deviceId, SystemCommandModel cmd)
        {
            var msg = JsonConvert.SerializeObject(cmd);
            _sbTasks.Add(_serviceClt.SendAsync(deviceId, new Message(Encoding.UTF8.GetBytes(msg))));
        }

        private async Task Listen(EventHubReceiver receiver)
        {
            while (!_tokenSrc.IsCancellationRequested)
            {
                _sbTasks.RemoveAll(x => x.IsCompleted);
                var eventData = await receiver.ReceiveAsync(TimeSpan.FromSeconds(30));
                if (eventData == null) continue;

                var data = Encoding.UTF8.GetString(eventData.GetBytes());

                var theEvent = JsonConvert.DeserializeObject<AllInOneModelDto>(data).ToFullModel() as AllInOneModel;

                var ctx = GlobalHost.ConnectionManager.GetHubContext<BusRHub>();
                ctx.Clients.All.eventReceived(theEvent?.ToString());
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing) return;

            _tokenSrc.Cancel(false);

            Task.WaitAll(_sbTasks.ToArray());

        }
    }
}