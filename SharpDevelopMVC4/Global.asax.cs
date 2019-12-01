/*
 * Created by SharpDevelop.
 * User: Gabs
 * Date: 19/07/2019
 * Time: 2:03 am
 */
 
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Newtonsoft.Json.Serialization;
using Hangfire.MemoryStorage;
using System.Web.Security;

namespace SharpDevelopMVC4
{
	public class MvcApplication : HttpApplication
	{
        Hangfire.BackgroundJobServer _backgroundJobServer;
        
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.Ignore("{resource}.axd/{*pathInfo}");
			
			routes.MapRoute(
				"Default",
				"{controller}/{action}/{id}",
				new {
					controller = "Home",
					action = "Index",
					id = UrlParameter.Optional
				});
		}

        protected void Application_Start()
		{
        	AutoMapperConfig.Initialize();
        	
			RegisterRoutes(RouteTable.Routes);
            
			// Configure Hangfire www.hangfire.io            
			Hangfire.GlobalConfiguration.Configuration.UseMemoryStorage();
			_backgroundJobServer = new Hangfire.BackgroundJobServer();         

			SimpleLogger.Init();
        }
        
        protected void Application_End(object sender, EventArgs e)
        {
            _backgroundJobServer.Dispose();
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket != null && !authTicket.Expired)
                {
                    var roles = authTicket.UserData.Split(',');
                    HttpContext.Current.User = new System.Security.Principal.GenericPrincipal(new FormsIdentity(authTicket), roles);
                }
            }
        }
    }
}
