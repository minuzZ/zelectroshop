using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Widgets.Countdown
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.Robokassa.ResultURL",
                 "Plugins/Robokassa/Result",
                 new { controller = "Robokassa", action = "Result" },
                 new[] { "Nop.Plugin.Payments.Robokassa.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.Robokassa.FailURL",
                 "Plugins/Robokassa/Fail",
                 new { controller = "Robokassa", action = "Fail" },
                 new[] { "Nop.Plugin.Payments.Robokassa.Controllers" }
            );
            routes.MapRoute("Plugin.Payments.Robokassa.SuccessURL",
                 "Plugins/Robokassa/Success",
                 new { controller = "Robokassa", action = "Success" },
                 new[] { "Nop.Plugin.Payments.Robokassa.Controllers" }
            );

        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
