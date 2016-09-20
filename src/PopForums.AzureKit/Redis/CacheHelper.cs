using System;
using Newtonsoft.Json;
using PopForums.Configuration;
using StackExchange.Redis;

namespace PopForums.AzureKit.Redis
{
    public class CacheHelper : ICacheHelper
    {
	    private readonly IConfig _config;
	    private static ConnectionMultiplexer _multiplexer;

	    public CacheHelper()
	    {
		    _config = new Config();
		    if (_multiplexer == null)
			    _multiplexer = ConnectionMultiplexer.Connect(_config.CacheConnectionString);
	    }

	    public void SetCacheObject(string key, object value)
	    {
		    var db = _multiplexer.GetDatabase();
		    var serialized = JsonConvert.SerializeObject(value);
		    db.StringSet(key, serialized, new TimeSpan(0, 0, _config.CacheSeconds));
	    }

	    public object GetCacheObject(string key)
		{
			var db = _multiplexer.GetDatabase();
			var result = db.StringGet(key);
			if (String.IsNullOrEmpty(result))
				return null;
			var deserialized = JsonConvert.DeserializeObject(result);
			return deserialized;
		}

	    public void RemoveCacheObject(string key)
		{
			var db = _multiplexer.GetDatabase();
			db.KeyDelete(key);
		}

	    public void SetCacheObject(string key, object value, double seconds)
		{
			var db = _multiplexer.GetDatabase();
			var serialized = JsonConvert.SerializeObject(value);
			db.StringSet(key, serialized, new TimeSpan(0, 0, (int)seconds));
		}

	    public T GetCacheObject<T>(string key)
		{
			var db = _multiplexer.GetDatabase();
			var result = db.StringGet(key);
			if (String.IsNullOrEmpty(result))
				return default(T);
			var deserialized = JsonConvert.DeserializeObject<T>(result);
			return deserialized;
		}
    }
}
