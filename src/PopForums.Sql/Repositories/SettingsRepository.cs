﻿namespace PopForums.Sql.Repositories;

public class SettingsRepository : ISettingsRepository
{
	public SettingsRepository(ISqlObjectFactory sqlObjectFactory, ICacheHelper cacheHelper)
	{
		_sqlObjectFactory = sqlObjectFactory;
		_cacheHelper = cacheHelper;
		_cacheHelper.OnRemoveCacheKey += key =>
		{
			var effectiveCacheKey = _cacheHelper.GetEffectiveCacheKey(CacheKey);
			if (key == effectiveCacheKey)
				OnSettingsInvalidated?.Invoke();
		};
	}

	private readonly ISqlObjectFactory _sqlObjectFactory;
	private readonly ICacheHelper _cacheHelper;
	private const string CacheKey = "pf.settings";

	public event Action OnSettingsInvalidated;

	public Dictionary<string, string> Get()
	{
		var cached = _cacheHelper.GetCacheObject<Dictionary<string, string>>(CacheKey);
		if (cached != null)
			return cached;
		var dictionary = new Dictionary<string, string>();
		_sqlObjectFactory.GetConnection().Using(connection =>
			dictionary = connection.Query("SELECT Setting, [Value] FROM pf_Setting").ToDictionary(r => (string)r.Setting, r => (string)r.Value));
		_cacheHelper.SetCacheObject(CacheKey, dictionary);
		return dictionary;
	}

	public void Save(Dictionary<string, object> dictionary)
	{
		_sqlObjectFactory.GetConnection().Using(connection =>
		{
			connection.Execute("DELETE FROM pf_Setting");
			foreach (var key in dictionary)
				connection.Execute("INSERT INTO pf_Setting (Setting, [Value]) VALUES (@Setting, @Value)", new { Setting = key.Key, Value = key.Value == null ? string.Empty : key.Value.ToString()});
		});
		_cacheHelper.RemoveCacheObject(CacheKey);
	}
}