using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using TrafficManager.Dashboard.Domain;
using TrafficManager.Domain.Models;
using TrafficManager.Domain.Models.Commands;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Dashboard.Hubs
{
    public class Transporter : ITransporter
    {
        private readonly IRepoDeviceMetadata _deviceRepo;
        private readonly CancellationTokenSource _tokenSrc = new CancellationTokenSource();
        private readonly ServiceClient _serviceClt;
        private readonly List<Task> _sbTasks = new List<Task>();

        public Transporter(IRepoDeviceMetadata deviceRepo)
        {
            _deviceRepo = deviceRepo;

            //HACK: Get this in to config!
            const string connectionString = "HostName=TrafficManager.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=d8fItWBBB2VlF5OxZn8cqwdBaw2MJUqtE4Lqoz5JhL8=";//"HostName=PieceOfPiHub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=jIUi1GLea8dDnwSu1j5N5fM/aJN7E4ubKxoRxUgUbGo=";
            const string iotHubToClientEndpoint = "messages/events";

            _serviceClt = ServiceClient.CreateFromConnectionString(connectionString);

            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubToClientEndpoint);

            foreach (var partition in eventHubClient.GetRuntimeInformation().PartitionIds)
            {
                //While debugging I found it helpful to backup the receiver a little to keep from having to constantly run the board
                //This allowed me to fire up the board every 15 minutes or as needed while developing the web
                var receiver = eventHubClient
                    .GetDefaultConsumerGroup()
                    .CreateReceiver(partition, DateTime.Now.AddMinutes(-15));
                _sbTasks.Add(Listen(receiver));
            }
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
                //Every 30 seconds, let's check for a cancellation. As far as I could tell, there is not a listen method that
                //has native cancellation support. There is for the normal Azure service bus, but guess it hasn't made it to 
                //the IoT hub libraries.
                var eventData = await receiver.ReceiveAsync(TimeSpan.FromSeconds(30));
                if (eventData == null) continue;

                var ctx = GlobalHost.ConnectionManager.GetHubContext<BusRHub>();
                var data = Encoding.UTF8.GetString(eventData.GetBytes());

                var theEvent = JsonConvert.DeserializeObject<AllInOneModelDto>(data).ToFullModel() as AllInOneModel;

                //Send the event
                if (theEvent == null)
                {
                    ctx.Clients.All.eventReceived(data);
                    continue;
                }

                var stream = (EventStreamEnum)theEvent.EventStream;

                ctx.Clients.All.eventReceived(theEvent.ToString(_deviceRepo));

                //If this is a summary event, trigger that method
                if (stream == EventStreamEnum.Summary)
                {
                    ctx.Clients.All.summaryUpdate(theEvent.ToString(_deviceRepo));
                    continue;
                }

                //If it's a state change
                if (stream != EventStreamEnum.StateChange) continue;

                //Let's get some more friendly device information
                var dev = _deviceRepo.GetByDeviceId(theEvent.DeviceId ?? theEvent.IntersectionId ?? Guid.Empty);

                //and trigger the stateChange method for our clients
                ctx.Clients.All.stateChange(dev.DeviceId, theEvent.CurrentState);

                //Finally the bulbChange method when appropriate to update the graphical UI
                if (dev?.DeviceType == "Bulb")
                    ctx.Clients.All.bulbChange(dev.DeviceId, theEvent.CurrentState == "On" || theEvent.CurrentState == "AssumedOn");
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