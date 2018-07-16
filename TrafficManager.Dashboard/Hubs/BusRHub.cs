using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using TrafficManager.Domain.Models.Commands;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Dashboard.Hubs
{
    public class BusRHub : Hub
    {
        private readonly Transporter _transporter;

        public BusRHub()
        {
	        _transporter = Transporter.Instance;
        }

        public void SendCommand(string userName, int commandEnumVal, Guid? targetId, string arg)
        {
            if (!targetId.HasValue) return;
            var cmd = (SystemCommandEnum) commandEnumVal;

	        var paramPairs = new SystemCommandParameters();

			
            if (cmd == SystemCommandEnum.UpdateRoutePreference)
                paramPairs.NewPreference = int.Parse(arg);

	        paramPairs.TargetId = targetId.Value;

            _transporter.SendCommand("PeiceOfPiDevice", new SystemCommandModel
            {
                Name = ((SystemCommandEnum)commandEnumVal).ToString(),
                CreatedTime = DateTime.UtcNow,
				Parameters = paramPairs
            });
        }
    }
}