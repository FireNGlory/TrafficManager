using Microsoft.Owin;
using Owin;
using TrafficManager.Dashboard;

[assembly: OwinStartup(typeof(Startup))]
namespace TrafficManager.Dashboard
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
