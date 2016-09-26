using System;
using System.Collections.Generic;

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
		/// Stores an object in cache as a group. When the root key is removed, all of the paged items 
		/// are also removed. Useful for storing paged threads and lists of topics.
		/// </summary>
		/// <typeparam name="T">The type for the collections stored together (like a List&lt;Post&gt;).</typeparam>
		/// <param name="rootKey">They key that references the collection of paged lists.</param>
		/// <param name="page">The page number of the collection to store.</param>
		/// <param name="value">The collection of T objects to cache.</param>
		void SetPagedListCacheObject<T>(string rootKey, int page, List<T> value);
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

		List<T> GetPagedListCacheObject<T>(string rootKey, int page);
	}
}