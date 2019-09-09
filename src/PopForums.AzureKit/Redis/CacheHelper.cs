using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using PopForums.Configuration;
using PopForums.Services;
using StackExchange.Redis;

namespace PopForums.AzureKit.Redis
{
    public class CacheHelper : ICacheHelper
    {
	    private readonly IErrorLog _errorLog;
	    private readonly ITenantService _tenantService;
	    private readonly IConfig _config;
	    private static ConnectionMultiplexer _cacheConnection;
	    private static ConnectionMultiplexer _messageConnection;
		private static IMemoryCache _cache;
		private static readonly object SyncRoot = new object();
		private const string RemoveChannel = "pf.cache.remove";

	    public CacheHelper(IErrorLog errorLog, ITenantService tenantService, IConfig config)
	    {
		    _errorLog = errorLog;
		    _tenantService = tenantService;
		    _config = config;
			// Redis cache
		    if (_cacheConnection == null)
		    {
			    lock (SyncRoot)
			    {
				    if (_cacheConnection == null)
						_cacheConnection = ConnectionMultiplexer.Connect(_config.CacheConnectionString);
			    }
		    }
			// Local cache
			if (_cache == null)
				SetupLocalCache();
			// Redis messaging to invalidate local cache entries
			if (_messageConnection == null)
		    {
			    lock (SyncRoot)
			    {
				    _messageConnection = ConnectionMultiplexer.Connect(_config.CacheConnectionString);
				    var db = _messageConnection.GetSubscriber();
				    db.Subscribe(RemoveChannel, (channel, value) =>
				    {
					    if (_cache == null)
						    return;
					    _cache.Remove(value.ToString());
				    });
			    }
		    }
	    }

	    private string PrefixTenantOnKey(string key)
	    {
		    var tenantID = _tenantService.GetTenant();
		    return $"{tenantID}:{key}";
	    }

	    private static void SetupLocalCache()
	    {
		    lock (SyncRoot)
		    {
			    var options = new MemoryCacheOptions();
			    _cache = new MemoryCache(options);
		    }
	    }

	    public void SetCacheObject(string key, object value)
	    {
		    SetCacheObject(key, value, _config.CacheSeconds);
		}

	    public void SetCacheObject(string key, object value, double seconds)
	    {
		    key = PrefixTenantOnKey(key);
			var timeSpan = TimeSpan.FromSeconds(seconds);
			var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = timeSpan };
			_cache.Set(key, value, options);
			try
			{
				var db = _cacheConnection.GetDatabase();
				var serialized = JsonConvert.SerializeObject(value);
				db.StringSet(key, serialized, timeSpan, flags: CommandFlags.FireAndForget);
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
			}
		}

	    public void SetLongTermCacheObject(string key, object value)
		{
			key = PrefixTenantOnKey(key);
			var options = new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(60) };
			_cache.Set(key, value, options);
			try
			{
				var db = _cacheConnection.GetDatabase();
				var serialized = JsonConvert.SerializeObject(value);
				db.StringSet(key, serialized, flags: CommandFlags.FireAndForget);
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
			}
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
			try
			{
				var db = _cacheConnection.GetDatabase();
				db.KeyDelete(key);
				var bus = _messageConnection.GetDatabase();
				bus.Publish(RemoveChannel, key, CommandFlags.FireAndForget);
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
			}
		}

	    public T GetCacheObject<T>(string key)
		{
			key = PrefixTenantOnKey(key);
			var cacheObject = _cache.Get(key);
			if (cacheObject != null)
				return (T)cacheObject;
			try
			{
				var db = _cacheConnection.GetDatabase();
				var result = db.StringGet(key);
				if (string.IsNullOrEmpty(result))
					return default;
				var deserialized = JsonConvert.DeserializeObject<T>(result);
				var timeSpan = TimeSpan.FromSeconds(_config.CacheSeconds);
				var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = timeSpan };
				_cache.Set(key, deserialized, options);
				return deserialized;
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
				return default;
			}
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
