using System.Web.Caching;

namespace PopForums.Configuration
{
	public interface ICacheHelper
	{
		Cache GetCache();
		void SetCacheObject(string key, object value);
		object GetCacheObject(string key);
		void RemoveCacheObject(string key);
		void SetCacheObject(string key, object value, double seconds);
		T GetCacheObject<T>(string key);
	}
}