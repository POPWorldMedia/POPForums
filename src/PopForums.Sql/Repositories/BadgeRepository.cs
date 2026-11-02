namespace PopForums.Sql.Repositories;

public class BadgeRepository : IBadgeRepository
{
	public BadgeRepository(ICacheHelper cacheHelper, ISqlObjectFactory sqlObjectFactory)
	{
		_cacheHelper = cacheHelper;
		_sqlObjectFactory = sqlObjectFactory;
	}

	private readonly ICacheHelper _cacheHelper;
	private readonly ISqlObjectFactory _sqlObjectFactory;

	public class CacheKeys
	{
		public const string AllBadges = "PopForums.Badges.All";
	}

	public async Task<List<Badge>> GetAll()
	{
		var cacheObject = _cacheHelper.GetCacheObject<List<Badge>>(CacheKeys.AllBadges);
		if (cacheObject != null)
			return cacheObject.ToList();
		Task<IEnumerable<Badge>> result = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			result = connection.QueryAsync<Badge>("SELECT BadgeID, Name, Role, ForegroundColor, BackgroundColor FROM pf_Badge ORDER BY BadgeID"));
		var list = result.Result.ToList();
		_cacheHelper.SetCacheObject(CacheKeys.AllBadges, list);
		return list.ToList();
	}

	public async Task<Badge> Get(int badgeID)
	{
		Task<Badge> badge = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			badge = connection.QuerySingleOrDefaultAsync<Badge>("SELECT BadgeID, Name, Role, ForegroundColor, BackgroundColor FROM pf_Badge WHERE BadgeID = @BadgeID", new { BadgeID = badgeID }));
		return await badge;
	}

	public async Task<int> Create(Badge badge)
	{
		Task<int> badgeID = null;
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			badgeID = connection.QuerySingleAsync<int>("INSERT INTO pf_Badge (Name, Role, ForegroundColor, BackgroundColor) VALUES (@Name, @Role, @ForegroundColor, @BackgroundColor);SELECT CAST(SCOPE_IDENTITY() as int)", new { badge.Name, badge.Role, badge.ForegroundColor, badge.BackgroundColor }));
		_cacheHelper.RemoveCacheObject(CacheKeys.AllBadges);
		return await badgeID;
	}

	public async Task Update(Badge badge)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("UPDATE pf_Badge SET Name = @Name, Role = @Role, ForegroundColor = @ForegroundColor, BackgroundColor = @BackgroundColor WHERE BadgeID = @BadgeID", new { badge.BadgeID, badge.Name, badge.Role, badge.ForegroundColor, badge.BackgroundColor }));
		_cacheHelper.RemoveCacheObject(CacheKeys.AllBadges);
	}

	public async Task Delete(int badgeID)
	{
		await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
			connection.ExecuteAsync("DELETE FROM pf_Badge WHERE BadgeID = @BadgeID", new { BadgeID = badgeID }));
		_cacheHelper.RemoveCacheObject(CacheKeys.AllBadges);
	}
}
