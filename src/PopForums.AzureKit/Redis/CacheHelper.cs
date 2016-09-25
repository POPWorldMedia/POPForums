using System;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using PopForums.Configuration;
using StackExchange.Redis;

namespace PopForums.AzureKit.Redis
{
    public class CacheHelper : ICacheHelper
    {
	    private readonly IErrorLog _errorLog;
	    private readonly IConfig _config;
	    private static ConnectionMultiplexer _cacheConnection;
	    private static ConnectionMultiplexer _messageConnection;
		private static IMemoryCache _cache;
		private const string _removeChannel = "pf.cache.remove";

	    public CacheHelper(IErrorLog errorLog)
	    {
		    _errorLog = errorLog;
		    _config = new Config();
			// Redis cache
		    if (_cacheConnection == null)
		    {
			    _cacheConnection = ConnectionMultiplexer.Connect(_config.CacheConnectionString);
			    _cacheConnection.PreserveAsyncOrder = false;
		    }
			// Local cache
			if (_cache == null)
				SetupLocalCache();
			// Redis messaging to invalidate local cache entries
			if (_messageConnection == null)
		    {
			    _messageConnection = ConnectionMultiplexer.Connect(_config.CacheConnectionString);
				_cacheConnection.PreserveAsyncOrder = false;
			    var db = _messageConnection.GetSubscriber();
			    db.Subscribe(_removeChannel, (channel, value) =>
				{
					if (_cache == null)
						return;
					_cache.Remove(value.ToString());
				});
		    }
	    }

	    private static void SetupLocalCache()
	    {
		    var options = new MemoryCacheOptions();
		    _cache = new MemoryCache(options);
	    }

	    public void SetCacheObject(string key, object value)
	    {
		    SetCacheObject(key, value, _config.CacheSeconds);
		}

	    public void SetCacheObject(string key, object value, double seconds)
		{
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

	    public void RemoveCacheObject(string key)
		{
			_cache.Remove(key);
			try
			{
				var db = _cacheConnection.GetDatabase();
				db.KeyDelete(key);
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
			}
		}

	    public T GetCacheObject<T>(string key)
		{
			var cacheObject = _cache.Get(key);
			if (cacheObject != null)
				return (T)cacheObject;
			try
			{
				var db = _cacheConnection.GetDatabase();
				var result = db.StringGet(key);
				if (String.IsNullOrEmpty(result))
					return default(T);
				var deserialized = JsonConvert.DeserializeObject<T>(result);
				var timeSpan = TimeSpan.FromSeconds(_config.CacheSeconds);
				var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = timeSpan };
				_cache.Set(key, deserialized, options);
				return deserialized;
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
				return default(T);
			}
		}
    }
}
