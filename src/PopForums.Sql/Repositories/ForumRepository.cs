using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class ForumRepository : IForumRepository
	{
		public ForumRepository(ICacheHelper cacheHelper, ISqlObjectFactory sqlObjectFactory)
		{
			_cacheHelper = cacheHelper;
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ICacheHelper _cacheHelper;
		private readonly ISqlObjectFactory _sqlObjectFactory;
		private const string ForumFields = "ForumID, CategoryID, Title, Description, IsVisible, IsArchived, SortOrder, TopicCount, PostCount, LastPostTime, LastPostName, UrlName, ForumAdapterName, IsQAForum";

		public class CacheKeys
		{
			public const string ForumPostRoleRestrictions = "PopForums.Forum.ForumPostRoleRestrictions";
			public const string ForumViewRoleRestrictions = "PopForums.Forum.ForumViewRoleRestrictions";
			public const string ForumUrlNames = "PopForums.Forum.UrlNames";
			public const string ForumTitles = "PopForums.Forum.Titles";
			public const string AggregateTopicCount = "PopForums.Forum.AggregateTopicCount";
			public const string AggregatePostCount = "PopForums.Forum.AggreatePostCount";
		}

		public async Task<Forum> Get(int forumID)
		{
			Task<Forum> forum = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				forum = connection.QuerySingleOrDefaultAsync<Forum>("SELECT " + ForumFields + " FROM pf_Forum WHERE ForumID = @ForumID", new { ForumID = forumID }));
			return await forum;
		}

		public async Task<Forum> Get(string urlName)
		{
			Task<Forum> forum = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				forum = connection.QuerySingleOrDefaultAsync<Forum>("SELECT " + ForumFields + " FROM pf_Forum WHERE UrlName = @UrlName", new { UrlName = urlName }));
			return await forum;
		}

		public async Task<Forum> Create(int? categoryID, string title, string description, bool isVisible, bool isArchived, int sortOrder, string urlName, string forumAdapterName, bool isQAForum)
		{
			if (categoryID == 0)
				categoryID = null;
			Task<int> forumID = null;
			var lastPostTime = new DateTime(2000, 1, 1);
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				forumID = connection.QuerySingleAsync<int>("INSERT INTO pf_Forum (CategoryID, Title, Description, IsVisible, IsArchived, SortOrder, TopicCount, PostCount, LastPostTime, LastPostName, UrlName, ForumAdapterName, IsQAForum) VALUES (@CategoryID, @Title, @Description, @IsVisible, @IsArchived, @SortOrder, 0, 0, @LastPostTime, '', @UrlName, @ForumAdapterName, @IsQAForum);SELECT CAST(SCOPE_IDENTITY() as int)", new { CategoryID = categoryID, Title = title, Description = description.NullToEmpty(), IsVisible = isVisible, IsArchived = isArchived, SortOrder = sortOrder, LastPostTime = lastPostTime, UrlName = urlName, ForumAdapterName = forumAdapterName, IsQAForum = isQAForum }));
			var forum = new Forum
			            	{
								ForumID = forumID.Result,
			            		CategoryID = categoryID,
			            		Title = title,
			            		Description = description,
			            		IsVisible = isVisible,
			            		IsArchived = isArchived,
			            		SortOrder = sortOrder,
			            		TopicCount = 0,
			            		PostCount = 0,
			            		LastPostTime = lastPostTime,
			            		LastPostName = String.Empty,
			            		UrlName = urlName,
								ForumAdapterName = forumAdapterName,
								IsQAForum = isQAForum
			            	};
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumUrlNames);
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumPostRoleRestrictions);
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumViewRoleRestrictions);
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumTitles);
			return forum;
		}

		public async Task<List<Forum>> GetForumsInCategory(int? categoryID)
		{
			Task<IEnumerable<Forum>> forums = null;
			if (categoryID.HasValue && categoryID != 0)
				await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
					forums = connection.QueryAsync<Forum>("SELECT " + ForumFields + " FROM pf_Forum WHERE CategoryID = @CategoryID", new { CategoryID = categoryID.Value }));
			else
				await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
					forums = connection.QueryAsync<Forum>("SELECT " + ForumFields + " FROM pf_Forum WHERE CategoryID = 0 OR CategoryID IS NULL"));
			return forums.Result.ToList();
		}

		public async Task<List<string>> GetUrlNamesThatStartWith(string urlName)
		{
			Task<IEnumerable<string>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<string>("SELECT UrlName FROM pf_Forum WHERE UrlName LIKE @UrlName + '%'", new { UrlName = urlName }));
			return list.Result.ToList();
		}

		public async Task Update(int forumID, int? categoryID, string title, string description, bool isVisible, bool isArchived, string urlName, string forumAdapterName, bool isQAForum)
		{
			if (categoryID == 0)
				categoryID = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_Forum SET CategoryID = @CategoryID, Title = @Title, Description = @Description, IsVisible = @IsVisible, IsArchived = @IsArchived, UrlName = @UrlName, ForumAdapterName = @ForumAdapterName, IsQAForum = @IsQAForum WHERE ForumID = @ForumID", new { CategoryID = categoryID, Title = title, Description = description.NullToEmpty(), IsVisible = isVisible, IsArchived = isArchived, UrlName = urlName, ForumAdapterName = forumAdapterName, IsQAForum = isQAForum, ForumID = forumID }));
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumUrlNames);
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumTitles);
		}

		public async Task UpdateSortOrder(int forumID, int newSortOrder)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_Forum SET SortOrder = @SortOrder WHERE ForumID = @ForumID", new { SortOrder = newSortOrder, ForumID = forumID }));
		}

		public async Task UpdateCategoryAssociation(int forumID, int? categoryID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_Forum SET CategoryID = @CategoryID WHERE ForumID = @ForumID", new { CategoryID = categoryID, ForumID = forumID }));
		}

		public async Task UpdateLastTimeAndUser(int forumID, DateTime lastTime, string lastName)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_Forum SET LastPostTime = @LastPostTime, LastPostName = @LastPostName WHERE ForumID = @ForumID", new { LastPostTime = lastTime, LastPostName = lastName, ForumID = forumID }));
		}

		public async Task UpdateTopicAndPostCounts(int forumID, int topicCount, int postCount)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_Forum SET TopicCount = @TopicCount, PostCount = @PostCount WHERE ForumID = @ForumID", new { TopicCount = topicCount, PostCount = postCount, ForumID = forumID }));
		}

		public async Task IncrementPostCount(int forumID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_Forum SET PostCount = PostCount + 1 WHERE ForumID = @ForumID", new { ForumID = forumID }));
		}

		public async Task IncrementPostAndTopicCount(int forumID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("UPDATE pf_Forum SET TopicCount = TopicCount + 1, PostCount = PostCount + 1 WHERE ForumID = @ForumID", new { ForumID = forumID }));
		}

		public async Task<IEnumerable<Forum>> GetAll()
		{
			Task<IEnumerable<Forum>> forums = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				forums = connection.QueryAsync<Forum>("SELECT " + ForumFields + " FROM pf_Forum ORDER BY SortOrder"));
			return await forums;
		}

		public async Task<IEnumerable<Forum>> GetAllVisible()
		{
			Task<IEnumerable<Forum>> forums = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				forums = connection.QueryAsync<Forum>("SELECT " + ForumFields + " FROM pf_Forum WHERE IsVisible = 1 ORDER BY SortOrder"));
			return await forums;
		}

		public async Task<List<string>> GetForumPostRoles(int forumID)
		{
			var restrictions = await GetForumPostRestrictionRoleGraph();
			var roles = restrictions.Single(r => r.Key == forumID).Value;
			return roles;
		}

		public async Task<List<string>> GetForumViewRoles(int forumID)
		{
			var restrictions = await GetForumViewRestrictionRoleGraph();
			var roles = restrictions.Single(r => r.Key == forumID).Value;
			return roles;
		}

		public async Task<Dictionary<int, List<string>>> GetForumPostRestrictionRoleGraph()
		{
			var cacheObject = _cacheHelper.GetCacheObject<Dictionary<int, List<string>>>(CacheKeys.ForumPostRoleRestrictions);
			if (cacheObject != null)
				return cacheObject;
			var dictionary = await GetForumRestrictionRoleGraph("pf_ForumPostRestrictions");
			_cacheHelper.SetLongTermCacheObject(CacheKeys.ForumPostRoleRestrictions, dictionary);
			return dictionary;
		}

		public async Task<Dictionary<int, List<string>>> GetForumViewRestrictionRoleGraph()
		{
			var cacheObject = _cacheHelper.GetCacheObject<Dictionary<int, List<string>>>(CacheKeys.ForumViewRoleRestrictions);
			if (cacheObject != null)
				return cacheObject;
			var dictionary = await GetForumRestrictionRoleGraph("pf_ForumViewRestrictions");
			_cacheHelper.SetLongTermCacheObject(CacheKeys.ForumViewRoleRestrictions, dictionary);
			return dictionary;
		}

		private async Task<Dictionary<int, List<string>>> GetForumRestrictionRoleGraph(string table)
		{
			var dictionary = new Dictionary<int, List<string>>();
			var forums = await GetAll();
			foreach (var forum in forums)
				dictionary.Add(forum.ForumID, new List<string>());
			IEnumerable<RoleGraph> roleGraph = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				roleGraph = connection.Query<RoleGraph>("SELECT ForumID, Role FROM " + table));
			foreach (var item in roleGraph)
			{
				dictionary.Single(d => d.Key == item.ForumID).Value
					.Add(item.Role);
			}
			return dictionary;
		}

		private class RoleGraph
		{
			public int ForumID { get; set; }
			public string Role { get; set; }
		}

		private async Task ModifyForumRole(int forumID, string role, string sql)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync(sql, new { ForumID = forumID, Role = role }));
		}

		private async Task ModifyForumRole(int forumID, string sql)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync(sql, new { ForumID = forumID }));
		}

		public async Task AddPostRole(int forumID, string role)
		{
			await RemovePostRole(forumID, role);
			await ModifyForumRole(forumID, role, "INSERT INTO pf_ForumPostRestrictions (ForumID, Role) VALUES (@ForumID, @Role)");
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumPostRoleRestrictions);
		}

		public async Task RemovePostRole(int forumID, string role)
		{
			await ModifyForumRole(forumID, role, "DELETE FROM pf_ForumPostRestrictions WHERE ForumID = @ForumID And Role = @Role");
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumPostRoleRestrictions);
		}

		public async Task AddViewRole(int forumID, string role)
		{
			await RemoveViewRole(forumID, role);
			await ModifyForumRole(forumID, role, "INSERT INTO pf_ForumViewRestrictions (ForumID, Role) VALUES (@ForumID, @Role)");
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumViewRoleRestrictions);
		}

		public async Task RemoveViewRole(int forumID, string role)
		{
			await ModifyForumRole(forumID, role, "DELETE FROM pf_ForumViewRestrictions WHERE ForumID = @ForumID And Role = @Role");
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumViewRoleRestrictions);
		}

		public async Task RemoveAllPostRoles(int forumID)
		{
			await ModifyForumRole(forumID, "DELETE FROM pf_ForumPostRestrictions WHERE ForumID = @ForumID");
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumPostRoleRestrictions);
		}

		public async Task RemoveAllViewRoles(int forumID)
		{
			await ModifyForumRole(forumID, "DELETE FROM pf_ForumViewRestrictions WHERE ForumID = @ForumID");
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumViewRoleRestrictions);
		}

		public async Task<IEnumerable<string>> GetAllForumUrlNames()
		{
			var cacheObject = _cacheHelper.GetCacheObject<IEnumerable<string>>(CacheKeys.ForumUrlNames);
			if (cacheObject != null)
				return cacheObject;
			Task<IEnumerable<string>> urlNames = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				urlNames = connection.QueryAsync<string>("SELECT UrlName FROM pf_Forum"));
			_cacheHelper.SetLongTermCacheObject(CacheKeys.ForumUrlNames, urlNames.Result);
			return urlNames.Result;
		}

		public Dictionary<int, string> GetAllForumTitles()
		{
			var cacheObject = _cacheHelper.GetCacheObject<Dictionary<int, string>>(CacheKeys.ForumTitles);
			if (cacheObject != null)
				return cacheObject;
			Dictionary<int, string> urlNames = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				urlNames = connection.Query("SELECT ForumID, Title FROM pf_Forum").ToDictionary(r => (int)r.ForumID, r => (string)r.Title));
			_cacheHelper.SetLongTermCacheObject(CacheKeys.ForumTitles, urlNames);
			return urlNames;
		}

		public async Task<int> GetAggregateTopicCount()
		{
			var cacheObject = _cacheHelper.GetCacheObject<int?>(CacheKeys.AggregateTopicCount);
			if (cacheObject != null)
				return cacheObject.Value;
			Task<int> count = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				count = connection.ExecuteScalarAsync<int>("SELECT SUM(TopicCount) FROM pf_Forum"));
			_cacheHelper.SetCacheObject(CacheKeys.AggregateTopicCount, count.Result);
			return count.Result;
		}

		public async Task<int> GetAggregatePostCount()
		{
			var cacheObject = _cacheHelper.GetCacheObject<int?>(CacheKeys.AggregatePostCount);
			if (cacheObject != null)
				return cacheObject.Value;
			Task<int> count = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				count = connection.ExecuteScalarAsync<int>("SELECT SUM(PostCount) FROM pf_Forum"));
			_cacheHelper.SetCacheObject(CacheKeys.AggregatePostCount, count.Result);
			return count.Result;
		}
	}
}
