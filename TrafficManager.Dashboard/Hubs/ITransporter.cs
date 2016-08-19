using System;
using TrafficManager.Domain.Models.Commands;

namespace TrafficManager.Dashboard.Hubs
{
    public interface ITransporter : IDisposable
    {
        void SendCommand(string deviceId, SystemCommandModel cmd);
    }
}