namespace PopForums.Configuration
{
    public class ConfigContainer
    {
		public string DatabaseConnectionString { get; set; }
		public int CacheSeconds { get; set; }
		public string CacheConnectionString { get; set; }
		public bool ForceLocalOnly { get; set; }
	}
}
