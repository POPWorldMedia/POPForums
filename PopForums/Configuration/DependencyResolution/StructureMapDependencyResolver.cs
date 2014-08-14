using System.Web.Http.Dependencies;
using StructureMap;
using IDependencyResolver = System.Web.Http.Dependencies.IDependencyResolver;

namespace PopForums.Configuration.DependencyResolution
{
	public class StructureMapDependencyResolver : StructureMapDependencyScope, IDependencyResolver
	{
		public StructureMapDependencyResolver(IContainer container)
			: base(container)
		{
		}
		public IDependencyScope BeginScope()
		{
			var child = Container.GetNestedContainer();
			return new StructureMapDependencyResolver(child);
		}
	}
}
