using System.Web.Http;
using Owin;

namespace PopForums.BackgroundWorker
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			var config = new HttpConfiguration();
			config.Routes.MapHttpRoute(
				"Default",
				"{controller}/{id}",
				new { id = RouteParameter.Optional });

			app.UseWebApi(config);
		}
	}
}