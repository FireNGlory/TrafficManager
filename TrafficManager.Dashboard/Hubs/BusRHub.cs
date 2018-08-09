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
			
	        int? newPref = null;
			
            if (cmd == SystemCommandEnum.UpdateRoutePreference)
	            newPref = int.Parse(arg);
			
            _transporter.SendCommand("PieceOfPiDevice", new SystemCommandModel
            {
                Name = ((SystemCommandEnum)commandEnumVal).ToString(),
				MessageId = Guid.NewGuid(),
                CreatedTime = DateTime.UtcNow,
				TargetId = targetId.Value,
				NewPreference = newPref
            });
        }
    }
}