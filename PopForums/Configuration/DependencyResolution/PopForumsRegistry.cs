using System.Web;
using Microsoft.Owin;
using PopForums.Controllers;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace PopForums.Configuration.DependencyResolution
{
	public class PopForumsRegistry : Registry
	{
		public PopForumsRegistry()
		{
			Scan(scan =>
				{
					scan.TheCallingAssembly();
					scan.WithDefaultConventions();
				});
			For<IOwinContext>().Use(x => HttpContext.Current.GetOwinContext());
			For<ForumController>().AlwaysUnique();
		}
	}
}