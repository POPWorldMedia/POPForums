using StructureMap;

namespace PopForums.Configuration.DependencyResolution
{
	public static class ContainerFactory
	{
		public static IContainer Initialize()
		{
			var container = new Container(x => x.AddRegistry<PopForumsRegistry>());
			return container;
		}
	}
}