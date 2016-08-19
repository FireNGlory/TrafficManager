using System;

namespace TrafficManager.Dashboard.Domain
{
    public interface IRepoDeviceMetadata
    {
        DeviceMetadata GetByDeviceId(Guid deviceId);
    }
}
