﻿using Autofac;
using Promact.Erp.Core.Controllers;
using Promact.Erp.Web.App_Start;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Promact.Erp.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var container = AutofacConfig.RegisterDependancies();
            DatabaseConfig.Initialize(container);
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Bot bot = container.Resolve<Bot>();
            bot.ScrumMain();
            bot.TaskMailBot();
        }
    }
}
