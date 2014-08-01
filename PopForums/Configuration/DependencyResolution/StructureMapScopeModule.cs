using System.Web;
using StructureMap.Web.Pipeline;

namespace PopForums.Configuration.DependencyResolution
{
	public class StructureMapScopeModule : IHttpModule
	{
		public void Dispose()
		{
		}

		public void Init(HttpApplication context)
		{
			context.BeginRequest += (sender, e) => StructuremapMvc.StructureMapDependencyScope.CreateNestedContainer();
			context.EndRequest += (sender, e) =>
			                      {
				                      HttpContextLifecycle.DisposeAndClearAll();
				                      StructuremapMvc.StructureMapDependencyScope.DisposeNestedContainer();
			                      };
		}
	}
}