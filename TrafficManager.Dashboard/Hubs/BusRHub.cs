using System;
using Microsoft.AspNet.SignalR;
using TrafficManager.Domain.Models.Commands;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Dashboard.Hubs
{
    public class BusRHub : Hub
    {
        public void SendCommand(string userName, int commandEnumVal, Guid? targetId, string arg)
        {
            if (!targetId.HasValue) return;

            Transporter.Instance().SendCommand("PieceOfPiDevice", new SystemCommandModel
            {
                TargetId = targetId.Value,
                FromUser = userName,
                RequestedCommand = (SystemCommandEnum)commandEnumVal,
                Arg1 = arg
            });
        }
    }
}