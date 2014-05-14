using System;
using Microsoft.ApplicationServer.Caching;
using PopForums.Configuration;

namespace PopForums.Data.Azure
{
	public class CacheHelper : ICacheHelper
	{
		public CacheHelper()
		{
			_config = new Config();
			var factory = new DataCacheFactory();
			_dataCache = factory.GetCache(CacheName);
		}

		private Config _config;
		private DataCache _dataCache;
		public const string CacheName = "PopForums";

		public void SetCacheObject(string key, object value)
		{
			_dataCache.Put(key, value, new TimeSpan(0, 0, 0, _config.CacheSeconds));
		}

		public object GetCacheObject(string key)
		{
			return _dataCache.Get(key);
		}

		public void RemoveCacheObject(string key)
		{
			_dataCache.Remove(key);
		}

		public void SetCacheObject(string key, object value, double seconds)
		{
			_dataCache.Put(key, value, new TimeSpan(0, 0, 0, (int)seconds));
		}

		public T GetCacheObject<T>(string key)
		{
			var cacheObject = _dataCache.Get(key);
			return (T)cacheObject;
		}
	}
}