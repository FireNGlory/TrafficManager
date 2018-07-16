using Microsoft.Owin;
using Owin;
using TrafficManager.Dashboard;
using TrafficManager.Dashboard.Hubs;

[assembly: OwinStartup(typeof(Startup))]
namespace TrafficManager.Dashboard
{
    public class Startup
    {
	    public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
	        var single = Transporter.Instance;
        }
    }
}
