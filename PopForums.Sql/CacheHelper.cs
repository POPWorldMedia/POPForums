using System;
using PopForums.Configuration;

namespace PopForums.Data.Sql
{
	public class CacheHelper : ICacheHelper
	{
		public CacheHelper()
		{
			//_config = new Config();
			//_cache = HttpRuntime.Cache;
		}

		//private readonly Config _config;
		//private readonly Cache _cache;

		public void SetCacheObject(string key, object value)
		{
			throw new NotImplementedException();
			//_cache.Insert(key, value, null, DateTime.Now.AddSeconds(_config.CacheSeconds), Cache.NoSlidingExpiration);
		}

		public void SetCacheObject(string key, object value, double seconds)
		{
			throw new NotImplementedException();
			//_cache.Insert(key, value, null, DateTime.Now.AddSeconds(seconds), Cache.NoSlidingExpiration);
		}

		public object GetCacheObject(string key)
		{
			throw new NotImplementedException();
			//return _cache.Get(key);
		}

		public T GetCacheObject<T>(string key)
		{
			throw new NotImplementedException();
			//var cacheObject = _cache.Get(key);
			//return cacheObject != null ? (T)cacheObject : default(T);
		}

		public void RemoveCacheObject(string key)
		{
			throw new NotImplementedException();
			//_cache.Remove(key);
		}
	}
}
