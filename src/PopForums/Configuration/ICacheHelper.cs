using System;

namespace PopForums.Configuration
{
	public interface ICacheHelper
	{
		/// <summary>
		/// Saves an object to cache using he configured number of seconds.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		void SetCacheObject(string key, object value);
		/// <summary>
		/// Saves an object to cache using the specified number of seconds.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="seconds"></param>
		void SetCacheObject(string key, object value, double seconds);
		/// <summary>
		/// Saves an object to cache without a time limit. Expiration should be deferred to 
		/// the cache mechanism.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		void SetLongTermCacheObject(string key, object value);
		/// <summary>
		/// Removes an object from cache.
		/// </summary>
		/// <param name="key"></param>
		void RemoveCacheObject(string key);
		/// <summary>
		/// Get an object from cache.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		T GetCacheObject<T>(string key);
	}
}