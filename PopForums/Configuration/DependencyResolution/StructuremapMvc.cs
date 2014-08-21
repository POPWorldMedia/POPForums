using StructureMap;

namespace PopForums.Configuration.DependencyResolution
{
	public static class StructuremapMvc
	{
		public static StructureMapDependencyScope StructureMapDependencyScope { get; set; }

		public static void End()
		{
			StructureMapDependencyScope.Dispose();
		}

		public static void Start()
		{
			var container = new Container(x => x.AddRegistry<PopForumsRegistry>());
			StructureMapDependencyScope = new StructureMapDependencyScope(container);
		}
	}
}