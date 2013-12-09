using Microsoft.Owin;
using Owin;
using PopForums.Configuration;
using PopForums.Web;

[assembly: OwinStartupAttribute(typeof(Startup))]
namespace PopForums.Web
{
	public partial class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			// fireup SignalR hubs
			app.MapSignalR();

			// run POP Forums OWIN stuff
			var pfOwin = new PopForumsOwinStartup();
			pfOwin.Configuration(app);
		}
	}
}