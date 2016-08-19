using Microsoft.AspNet.SignalR.Hubs;
using StructureMap;

namespace TrafficManager.Dashboard.DependencyResolution
{
    public class SigRActivator : IHubActivator
    {
        private readonly Container _container;

        public SigRActivator(Container container)
        {
            _container = container;
        }
        public IHub Create(HubDescriptor descriptor)
        {
            return (IHub)_container.GetInstance(descriptor.HubType);
        }
    }
}