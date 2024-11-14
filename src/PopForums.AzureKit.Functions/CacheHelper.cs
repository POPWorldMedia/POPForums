using System;
using System.Collections.Generic;
using PopForums.Configuration;

namespace PopForums.AzureKit.Functions;

public class CacheHelper : ICacheHelper
{
	public void SetCacheObject(string key, object value)
	{
	}

	public void SetCacheObject(string key, object value, double seconds)
	{
	}

	public void SetLongTermCacheObject(string key, object value)
	{
	}

	public void SetPagedListCacheObject<T>(string rootKey, int page, List<T> value)
	{
	}

	public void RemoveCacheObject(string key)
	{
	}

	public T GetCacheObject<T>(string key)
	{
		return default;
	}

	public List<T> GetPagedListCacheObject<T>(string rootKey, int page)
	{
		return null;
	}

	public event Action<string> OnRemoveCacheKey;
	
	public string GetEffectiveCacheKey(string key)
	{
		return key;
	}
}
