namespace PopForums.Configuration
{
	public interface IConfig
	{
		string DatabaseConnectionString { get; }
		int CacheSeconds { get; }
		string CacheConnectionString { get; }
		bool ForceLocalOnly { get; }
		string SearchUrl { get; }
		string SearchKey { get; }
		string QueueConnectionString { get; }
		string SearchProvider { get; }
		bool LogTopicViews { get; }
		bool UseReCaptcha { get; }
		string ReCaptchaSiteKey { get; }
		string ReCaptchaSecretKey { get; }
		bool ExternalLoginOnly { get; }
	}

	public class Config : IConfig
	{
		public Config()
		{
			if (_configContainer == null)
			{
				var loader = new ConfigLoader();
				_configContainer = loader.GetConfig(_basePath, _configFileName);
			}
		}

		public static void SetPopForumsAppEnvironment(string basePath, string configFileName = "PopForums.json")
		{
			_basePath = basePath;
			_configFileName = configFileName;
		}

		private static string _basePath;
		private static string _configFileName;
		private static ConfigContainer _configContainer;

		public string DatabaseConnectionString => _configContainer.DatabaseConnectionString;
		public int CacheSeconds => _configContainer.CacheSeconds;
		public string CacheConnectionString => _configContainer.CacheConnectionString;
		public bool ForceLocalOnly => _configContainer.CacheForceLocalOnly;
		public string SearchUrl => _configContainer.SearchUrl;
		public string SearchKey => _configContainer.SearchKey;
		public string QueueConnectionString => _configContainer.QueueConnectionString;
		public string SearchProvider => _configContainer.SearchProvider;
		public bool LogTopicViews => _configContainer.LogTopicViews;
		public bool UseReCaptcha => _configContainer.UseReCaptcha;
		public string ReCaptchaSiteKey => _configContainer.ReCaptchaSiteKey;
		public string ReCaptchaSecretKey => _configContainer.ReCaptchaSecretKey;
		public bool ExternalLoginOnly => _configContainer.ExternalLoginOnly;
	}
}
