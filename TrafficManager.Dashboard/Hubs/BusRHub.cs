using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using TrafficManager.Domain.Models.Commands;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Dashboard.Hubs
{
    public class BusRHub : Hub
    {
        private readonly ITransporter _transporter;

        public BusRHub(ITransporter transporter)
        {
            _transporter = transporter;
        }

        public void SendCommand(string userName, int commandEnumVal, Guid? targetId, string arg)
        {
            if (!targetId.HasValue) return;
            var cmd = (SystemCommandEnum) commandEnumVal;

            var paramPairs = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("targetId", targetId),
            };

            if (cmd == SystemCommandEnum.UpdateRoutePreference)
                paramPairs.Add(new KeyValuePair<string, object>("preference", int.Parse(arg)));

            _transporter.SendCommand("PieceOfPiDevice", new SystemCommandModel
            {
                Name = ((SystemCommandEnum)commandEnumVal).ToString(),
                CreatedTime = DateTime.UtcNow,
                Parameters = paramPairs
            });
        }
    }
}