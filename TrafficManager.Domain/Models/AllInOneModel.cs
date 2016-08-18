using System;
using TrafficManager.Domain.Reference;

namespace TrafficManager.Domain.Models
{
    public class AllInOneModel : IotHubModelBase
    {
        public Guid? DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string DeviceName { get; set; }
        public Guid? IntersectionId { get; set; }
        public string Function { get; set; }
        public string Description { get; set; }
        public string CurrentState { get; set; }
        public Guid? ParentDeviceId { get; set; }
        public bool IsError { get; set; }
        public string Message { get; set; }
        public string OldState { get; set; }
        public string NewState { get; set; }
        public decimal UsageFactorOne { get; set; }
        public decimal UsageFactorTwo { get; set; }
        
        public override string ToString()
        {
            int cs;
            int.TryParse(CurrentState, out cs);

            if (cs > 0)
            {
                if (Enum.IsDefined(typeof(BulbStateEnum), cs))
                    CurrentState = ((BulbStateEnum)cs).ToString();
                if (Enum.IsDefined(typeof(CurrentSensorStateEnum), cs))
                    CurrentState = ((CurrentSensorStateEnum)cs).ToString();
                if (Enum.IsDefined(typeof(IntersectionStateEnum), cs))
                    CurrentState = ((IntersectionStateEnum)cs).ToString();
                if (Enum.IsDefined(typeof(LampStateEnum), cs))
                    CurrentState = ((LampStateEnum)cs).ToString();
                if (Enum.IsDefined(typeof(RightOfWayStateEnum), cs))
                    CurrentState = ((RightOfWayStateEnum)cs).ToString();

            }

            var stream = (EventStreamEnum)EventStream;
            var errorNote = IsError ? " ***ERROR*** " : "";
            var msg =
                $"{Timestamp.ToString("yyyy-MM-dd HH:mm:ss")}Z - {errorNote}{stream}: {DeviceType}({DeviceId ?? IntersectionId})";
            if (!string.IsNullOrWhiteSpace(Function)) msg = string.Concat(msg, $" - Function: {Function}");
            if (!string.IsNullOrWhiteSpace(Description)) msg = string.Concat(msg, $" - Description: {Description}");
            if (!string.IsNullOrWhiteSpace(CurrentState)) msg = string.Concat(msg, $" - CurrentState: {CurrentState}");
            if (ParentDeviceId.HasValue) msg = string.Concat(msg, $" - ParentDeviceId: {ParentDeviceId}");
            if (ParentDeviceId.HasValue) msg = string.Concat(msg, $" - ParentDeviceId: {ParentDeviceId}");
            if (!string.IsNullOrWhiteSpace(Message)) msg = string.Concat(msg, $" - Message: {Message}");
            if (!string.IsNullOrWhiteSpace(OldState)) msg = string.Concat(msg, $" - OldState: {OldState}");
            if (!string.IsNullOrWhiteSpace(NewState)) msg = string.Concat(msg, $" - NewState: {NewState}");
            if (UsageFactorOne != 0) msg = string.Concat(msg, $" - UsageFactorOne: {UsageFactorOne}");
            if (UsageFactorTwo != 0) msg = string.Concat(msg, $" - UsageFactorTwo: {UsageFactorTwo}");
            return msg;
        }
    }
}
