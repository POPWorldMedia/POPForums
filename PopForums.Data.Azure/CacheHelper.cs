using System;
using System.Configuration;
using Newtonsoft.Json;
using PopForums.Configuration;
using StackExchange.Redis;

namespace PopForums.Data.Azure
{
	public class CacheHelper : ICacheHelper
	{
		private readonly IErrorLog _errorLog;

		public CacheHelper(IErrorLog errorLog)
		{
			_errorLog = errorLog;
			_config = new Config();
			if (connection == null)
			{
				var connectionString = ConfigurationManager.ConnectionStrings[_config.CacheConnectionStringName].ConnectionString;
				if (String.IsNullOrWhiteSpace(connectionString))
					throw new Exception(String.Format("Can't find a connnection string named '{0}' for the CacheConnectionStringName.", _config.CacheConnectionStringName));
				connection = ConnectionMultiplexer.Connect(connectionString);
			}
			_cache = connection.GetDatabase();
		}

		private static ConnectionMultiplexer connection;

		private Config _config;
		private IDatabase _cache;

		public void SetCacheObject(string key, object value)
		{
			SetCacheObject(key, value, _config.CacheSeconds);
		}

		[Obsolete("User GetCacheObject<T>(key) instead.")]
		public object GetCacheObject(string key)
		{
			var v = _cache.StringGet(key);
			return v;
		}

		public void RemoveCacheObject(string key)
		{
			_cache.KeyDelete(key);
		}

		public void SetCacheObject(string key, object value, double seconds)
		{
			if (value == null)
				return;
			try
			{
				var v = JsonConvert.SerializeObject(value, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
				_cache.StringSet(key, v, new TimeSpan(0, 0, 0, (int) seconds));
			}
			catch (TimeoutException exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Information);
			}
		}

		public T GetCacheObject<T>(string key)
		{
			try
			{
				var v = _cache.StringGet(key);
				if (v.IsNullOrEmpty)
					return default(T);
				var value = JsonConvert.DeserializeObject<T>(v);
				return value;
			}
			catch (TimeoutException exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Information);
				return default (T);
			}
		}
	}
}