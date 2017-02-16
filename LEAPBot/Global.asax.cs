using LEAPBot.IoC;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace LEAPBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static Container Container = new Container(new RuntimeRegistry());

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
