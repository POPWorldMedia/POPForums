using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject;
using PopForums.Data.SqlSingleWebServer;

namespace PopForums.Web
{
	public class MvcApplication : HttpApplication
	{
		protected void Application_Start()
		{
			// this registers the SQL single Web server repositories for Ninject
			PopForumsActivation.Kernel.Load(new SqlSingleInjectionModule());

			// Sets global filter to set a PopForums.Model.User in the pipeline as an IPrincipal
			PopForumsActivation.SetUserAttribute();
			// Remove the previous line and uncomment the following to apply PopForums users to all controllers in the app
			// PopForumsActivation.SetGlobalUserAttribute();

			// Got other controller namespaces to register in the PopForums routing and area? (For forum adapters) Set them here.
			// PopForumsActivation.SetAdditionalControllerNamespaces(new[] {"MyNamespace"});

			// Run the background services in this Web application.
			PopForumsActivation.StartServices();

			// Register SignalR hubs
			RouteTable.Routes.MapHubs();

			RegisterRoutes(RouteTable.Routes);
		}

		protected void RegisterRoutes(RouteCollection routes)
		{
			AreaRegistration.RegisterAllAreas();
			routes.IgnoreRoute("*.ico");
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			// Your app routes here
			// routes.MapRoute("Default", "{controller}/{action}/{id}", new {controller = PopForums.Controllers.ForumHomeController.Name, action = "Index", id = UrlParameter.Optional});
		}
	}
}