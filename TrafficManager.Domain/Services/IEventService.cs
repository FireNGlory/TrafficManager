using System;
using System.Collections.Generic;
using TrafficManager.Domain.EventHandlers;
using TrafficManager.Domain.Reference;
using TrafficManager.Domain.ValueTypes;

namespace TrafficManager.Domain.Services
{
    public interface IEventService : IDisposable
    {
        event CommandReceivedEvent CommandReceived;

        void SendOnline();
        void UpdateDirectory(Guid deviceId, string deviceType, string deviceName, Guid? parentId);
        void SendStateChangeEvent(Guid deviceId, string deviceType, string oldState, string newState, DateTime timestamp);
        void SendAnomaly(Guid intersectionId, string function, string desc, Guid? offender, DateTime timestamp);
        void SendSummaryUpdates(ICollection<DeviceSummary> summaries);
        void SendUsageUpdate(Guid deviceId, decimal factorOne, decimal factorTwo);
        void SendRightOfWayChanged(Guid intersectionId, Guid oldRoWRouteId, Guid newRoWRouteId, DateTime timeStamp);
        void SendLogMessage(Guid intersectionId, bool containsError, string desc, DateTime timeStamp);
    }
}
