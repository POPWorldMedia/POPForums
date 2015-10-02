using System;
using Microsoft.Framework.Caching.Memory;
using PopForums.Configuration;

namespace PopForums.Data.Sql
{
	public class CacheHelper : ICacheHelper
	{
		public CacheHelper()
		{
			_config = new Config();
			if (_cache == null)
			{
				var options = new MemoryCacheOptions();
				_cache = new MemoryCache(options);
			}
		}

		private readonly Config _config;
		private static IMemoryCache _cache;

		public void SetCacheObject(string key, object value)
		{
			var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_config.CacheSeconds) };
			_cache.Set(key, value, options);
		}

		public void SetCacheObject(string key, object value, double seconds)
		{
			var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(seconds) };
			_cache.Set(key, value, options);
		}

		public object GetCacheObject(string key)
		{
			return _cache.Get(key);
		}

		public T GetCacheObject<T>(string key)
		{
			var cacheObject = _cache.Get(key);
			return cacheObject != null ? (T)cacheObject : default(T);
		}

		public void RemoveCacheObject(string key)
		{
			_cache.Remove(key);
		}
	}
}
