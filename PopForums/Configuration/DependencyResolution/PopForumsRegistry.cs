using System.Web;
using Microsoft.Owin;
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
					scan.With(new ControllerConvention());
				});
			For<IOwinContext>().Use(x => HttpContext.Current.GetOwinContext());
		}
	}
}