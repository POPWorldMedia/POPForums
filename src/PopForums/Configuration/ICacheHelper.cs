using System;

namespace PopForums.Configuration
{
	public interface ICacheHelper
	{
		void SetCacheObject(string key, object value);
		void SetCacheObject(string key, object value, double seconds);
		void SetLongTermCacheObject(string key, object value);
		void RemoveCacheObject(string key);
		void RemoveLongTermCacheObject(string key);
		T GetCacheObject<T>(string key);
		T GetLongTermCacheObject<T>(string key);
	}
}