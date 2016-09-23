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
	    private const string _addChannel = "pf.cache.add";

	    public CacheHelper(IErrorLog errorLog)
	    {
		    _errorLog = errorLog;
		    _config = new Config();
		    if (_cacheConnection == null)
		    {
			    _cacheConnection = ConnectionMultiplexer.Connect(_config.CacheConnectionString);
			    _cacheConnection.PreserveAsyncOrder = false;
		    }
			if (_cache == null)
				SetupLocalCache();
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
			    db.Subscribe(_addChannel, (channel, value) =>
				{
					if (_cache == null)
						SetupLocalCache();
					var keyValue = JsonConvert.DeserializeObject<LocalCacheKeyValue>(value);
					var options = new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(60) };
					var valueType = Type.GetType(keyValue.FullType);
					var castValue = JsonConvert.DeserializeObject(keyValue.Value.ToString(), valueType);
					_cache.Set(keyValue.Key, castValue, options);
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
			try
			{ 
				var db = _cacheConnection.GetDatabase();
				var serialized = JsonConvert.SerializeObject(value);
				db.StringSet(key, serialized, new TimeSpan(0, 0, _config.CacheSeconds));
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
			}
		}

	    public void SetLongTermCacheObject(string key, object value)
		{
			try
			{
				var db = _messageConnection.GetDatabase();
				var keyValue = new LocalCacheKeyValue { Key = key, Value = value, FullType = value.GetType().FullName };
				var serialized = JsonConvert.SerializeObject(keyValue);
				db.Publish(_addChannel, serialized);
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
			}
		}

	    public void SetCacheObject(string key, object value, double seconds)
		{
			try
			{
				var db = _cacheConnection.GetDatabase();
				var serialized = JsonConvert.SerializeObject(value);
				db.StringSet(key, serialized, new TimeSpan(0, 0, (int)seconds));
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
			}
		}

	    public void RemoveCacheObject(string key)
		{
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

	    public void RemoveLongTermCacheObject(string key)
	    {
		    try
			{
				var db = _messageConnection.GetSubscriber();
				db.Publish(_removeChannel, key);
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
			}
		}

	    public T GetCacheObject<T>(string key)
		{
			try
			{
				var db = _cacheConnection.GetDatabase();
				var result = db.StringGet(key);
				if (String.IsNullOrEmpty(result))
					return default(T);
				var deserialized = JsonConvert.DeserializeObject<T>(result);
				return deserialized;
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
				return default(T);
			}
		}

	    public T GetLongTermCacheObject<T>(string key)
		{
			var cacheObject = _cache.Get(key);
			return cacheObject != null ? (T)cacheObject : default(T);
		}

	    private class LocalCacheKeyValue
	    {
		    public string Key { get; set; }
			public object Value { get; set; }
			public string FullType { get; set; }
	    }
    }
}
