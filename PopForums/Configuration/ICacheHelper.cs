namespace PopForums.Configuration
{
	public interface ICacheHelper
	{
		void SetCacheObject(string key, object value);
		object GetCacheObject(string key);
		void RemoveCacheObject(string key);
		void SetCacheObject(string key, object value, double seconds);
		T GetCacheObject<T>(string key);
	}
}