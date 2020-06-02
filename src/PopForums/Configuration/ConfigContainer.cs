namespace PopForums.Configuration
{
	public class ConfigContainer
	{
		public string DatabaseConnectionString { get; set; }
		public int CacheSeconds { get; set; }
		public string CacheConnectionString { get; set; }
		public bool CacheForceLocalOnly { get; set; }
		public string SearchUrl { get; set; }
		public string SearchKey { get; set; }
		public string QueueConnectionString { get; set; }
		public string SearchProvider { get; set; }
		public bool LogTopicViews { get; set; }
		public bool UseReCaptcha { get; set; }
		public string ReCaptchaSiteKey { get; set; }
		public string ReCaptchaSecretKey { get; set; }
		public bool ExternalLoginOnly { get; set; }
	}
}
