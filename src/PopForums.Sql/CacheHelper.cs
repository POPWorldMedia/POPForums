using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.Sql
{
	public class CacheHelper : ICacheHelper
	{
		public CacheHelper(IConfig config, ITenantService tenantService)
		{
			_config = config;
			_tenantService = tenantService;
			if (_cache == null)
			{
				var options = new MemoryCacheOptions();
				_cache = new MemoryCache(options);
			}
		}

		private readonly IConfig _config;
		private readonly ITenantService _tenantService;
		private static IMemoryCache _cache;

		private string PrefixTenantOnKey(string key)
		{
			var tenantID = _tenantService.GetTenant();
			return $"{tenantID}:{key}";
		}

		public void SetCacheObject(string key, object value)
		{
			key = PrefixTenantOnKey(key);
			var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_config.CacheSeconds) };
			_cache.Set(key, value, options);
		}

		public void SetCacheObject(string key, object value, double seconds)
		{
			key = PrefixTenantOnKey(key);
			var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(seconds) };
			_cache.Set(key, value, options);
		}

		public void SetLongTermCacheObject(string key, object value)
		{
			key = PrefixTenantOnKey(key);
			var options = new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(60) };
			_cache.Set(key, value, options);
		}

		public void SetPagedListCacheObject<T>(string rootKey, int page, List<T> value)
		{
			rootKey = PrefixTenantOnKey(rootKey);
			_cache.TryGetValue(rootKey, out Dictionary<int, List<T>> rootPages);
			if (rootPages == null)
				rootPages = new Dictionary<int, List<T>>();
			else if (rootPages.ContainsKey(page))
				rootPages.Remove(page);
			rootPages.Add(page, value);
			var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_config.CacheSeconds) };
			_cache.Set(rootKey, rootPages, options);
		}

		public void RemoveCacheObject(string key)
		{
			key = PrefixTenantOnKey(key);
			_cache.Remove(key);
		}

		public T GetCacheObject<T>(string key)
		{
			key = PrefixTenantOnKey(key);
			var cacheObject = _cache.Get(key);
			return cacheObject != null ? (T)cacheObject : default;
		}

		public List<T> GetPagedListCacheObject<T>(string rootKey, int page)
		{
			rootKey = PrefixTenantOnKey(rootKey);
			_cache.TryGetValue(rootKey, out Dictionary<int, List<T>> rootPages);
			if (rootPages == null)
				return null;
			if (rootPages.ContainsKey(page))
				return rootPages[page];
			return null;
		}
	}
}
