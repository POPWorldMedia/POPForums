using System;
using System.Web;
using System.Web.Caching;
using PopForums.Configuration;

namespace PopForums.Data.SqlSingleWebServer
{
	public class CacheHelper : ICacheHelper
	{
		public CacheHelper()
		{
			_config = new Config();
			_cache = HttpRuntime.Cache;
		}

		private readonly Config _config;
		private readonly Cache _cache;

		public void SetCacheObject(string key, object value)
		{
			_cache.Insert(key, value, null, DateTime.Now.AddSeconds(_config.CacheSeconds), Cache.NoSlidingExpiration);
		}

		public void SetCacheObject(string key, object value, double seconds)
		{
			_cache.Insert(key, value, null, DateTime.Now.AddSeconds(seconds), Cache.NoSlidingExpiration);
		}

		public object GetCacheObject(string key)
		{
			return _cache.Get(key);
		}

		public T GetCacheObject<T>(string key)
		{
			var cacheObject = _cache.Get(key);
			return (T) cacheObject;
		}

		public void RemoveCacheObject(string key)
		{
			_cache.Remove(key);
		}
	}
}
