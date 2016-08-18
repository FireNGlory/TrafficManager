using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TrafficManager.Dashboard.Hubs;

namespace TrafficManager.Dashboard
{
    public class MvcApplication : HttpApplication
    {
        private Transporter _busHub;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            _busHub = Transporter.Instance();
        }

        protected void Application_End()
        {
            _busHub.Dispose();
        }
    }
}
