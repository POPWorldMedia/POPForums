using System;
using System.Collections.Generic;
using System.Linq;
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

		public Forum Get(int forumID)
		{
			Forum forum = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				forum = connection.QuerySingleOrDefault<Forum>("SELECT " + ForumFields + " FROM pf_Forum WHERE ForumID = @ForumID", new { ForumID = forumID }));
			return forum;
		}

		public Forum Get(string urlName)
		{
			Forum forum = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				forum = connection.QuerySingleOrDefault<Forum>("SELECT " + ForumFields + " FROM pf_Forum WHERE UrlName = @UrlName", new { UrlName = urlName }));
			return forum;
		}

		public Forum Create(int? categoryID, string title, string description, bool isVisible, bool isArchived, int sortOrder, string urlName, string forumAdapterName, bool isQAForum)
		{
			if (categoryID == 0)
				categoryID = null;
			var forumID = 0;
			var lastPostTime = new DateTime(2000, 1, 1);
			_sqlObjectFactory.GetConnection().Using(connection => 
				forumID = connection.QuerySingle<int>("INSERT INTO pf_Forum (CategoryID, Title, Description, IsVisible, IsArchived, SortOrder, TopicCount, PostCount, LastPostTime, LastPostName, UrlName, ForumAdapterName, IsQAForum) VALUES (@CategoryID, @Title, @Description, @IsVisible, @IsArchived, @SortOrder, 0, 0, @LastPostTime, '', @UrlName, @ForumAdapterName, @IsQAForum);SELECT CAST(SCOPE_IDENTITY() as int)", new { CategoryID = categoryID, Title = title, Description = description.NullToEmpty(), IsVisible = isVisible, IsArchived = isArchived, SortOrder = sortOrder, LastPostTime = lastPostTime, UrlName = urlName, ForumAdapterName = forumAdapterName, IsQAForum = isQAForum }));
			var forum = new Forum
			            	{
								ForumID = forumID,
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

		public List<Forum> GetForumsInCategory(int? categoryID)
		{
			var forums = new List<Forum>();
			if (categoryID.HasValue && categoryID != 0)
				_sqlObjectFactory.GetConnection().Using(connection =>
					forums = connection.Query<Forum>("SELECT " + ForumFields + " FROM pf_Forum WHERE CategoryID = @CategoryID", new { CategoryID = categoryID.Value }).ToList());
			else
				_sqlObjectFactory.GetConnection().Using(connection =>
					forums = connection.Query<Forum>("SELECT " + ForumFields + " FROM pf_Forum WHERE CategoryID = 0 OR CategoryID IS NULL").ToList());
			return forums;
		}

		public List<string> GetUrlNamesThatStartWith(string urlName)
		{
			var list = new List<string>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<string>("SELECT UrlName FROM pf_Forum WHERE UrlName LIKE @UrlName + '%'", new { UrlName = urlName }).ToList());
			return list;
		}

		public void Update(int forumID, int? categoryID, string title, string description, bool isVisible, bool isArchived, string urlName, string forumAdapterName, bool isQAForum)
		{
			if (categoryID == 0)
				categoryID = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("UPDATE pf_Forum SET CategoryID = @CategoryID, Title = @Title, Description = @Description, IsVisible = @IsVisible, IsArchived = @IsArchived, UrlName = @UrlName, ForumAdapterName = @ForumAdapterName, IsQAForum = @IsQAForum WHERE ForumID = @ForumID", new { CategoryID = categoryID, Title = title, Description = description.NullToEmpty(), IsVisible = isVisible, IsArchived = isArchived, UrlName = urlName, ForumAdapterName = forumAdapterName, IsQAForum = isQAForum, ForumID = forumID }));
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumUrlNames);
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumTitles);
		}

		public void UpdateSortOrder(int forumID, int newSortOrder)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("UPDATE pf_Forum SET SortOrder = @SortOrder WHERE ForumID = @ForumID", new { SortOrder = newSortOrder, ForumID = forumID }));
		}

		public void UpdateCategoryAssociation(int forumID, int? categoryID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("UPDATE pf_Forum SET CategoryID = @CategoryID WHERE ForumID = @ForumID", new { CategoryID = categoryID, ForumID = forumID }));
		}

		public void UpdateLastTimeAndUser(int forumID, DateTime lastTime, string lastName)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("UPDATE pf_Forum SET LastPostTime = @LastPostTime, LastPostName = @LastPostName WHERE ForumID = @ForumID", new { LastPostTime = lastTime, LastPostName = lastName, ForumID = forumID }));
		}

		public void UpdateTopicAndPostCounts(int forumID, int topicCount, int postCount)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("UPDATE pf_Forum SET TopicCount = @TopicCount, PostCount = @PostCount WHERE ForumID = @ForumID", new { TopicCount = topicCount, PostCount = postCount, ForumID = forumID }));
		}

		public void IncrementPostCount(int forumID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("UPDATE pf_Forum SET PostCount = PostCount + 1 WHERE ForumID = @ForumID", new { ForumID = forumID }));
		}

		public void IncrementPostAndTopicCount(int forumID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("UPDATE pf_Forum SET TopicCount = TopicCount + 1, PostCount = PostCount + 1 WHERE ForumID = @ForumID", new { ForumID = forumID }));
		}

		public IEnumerable<Forum> GetAll()
		{
			var forums = new List<Forum>();
			_sqlObjectFactory.GetConnection().Using(connection => 
				forums = connection.Query<Forum>("SELECT " + ForumFields + " FROM pf_Forum ORDER BY SortOrder").ToList());
			return forums;
		}

		public IEnumerable<Forum> GetAllVisible()
		{
			var forums = new List<Forum>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				forums = connection.Query<Forum>("SELECT " + ForumFields + " FROM pf_Forum WHERE IsVisible = 1 ORDER BY SortOrder").ToList());
			return forums;
		}

		public List<string> GetForumPostRoles(int forumID)
		{
			var restrictions = GetForumPostRestrictionRoleGraph();
			var roles = restrictions.Single(r => r.Key == forumID).Value;
			return roles;
		}

		public List<string> GetForumViewRoles(int forumID)
		{
			var restrictions = GetForumViewRestrictionRoleGraph();
			var roles = restrictions.Single(r => r.Key == forumID).Value;
			return roles;
		}

		public Dictionary<int, List<string>> GetForumPostRestrictionRoleGraph()
		{
			var cacheObject = _cacheHelper.GetCacheObject<Dictionary<int, List<string>>>(CacheKeys.ForumPostRoleRestrictions);
			if (cacheObject != null)
				return cacheObject;
			var dictionary = GetForumRestrictionRoleGraph("pf_ForumPostRestrictions");
			_cacheHelper.SetLongTermCacheObject(CacheKeys.ForumPostRoleRestrictions, dictionary);
			return dictionary;
		}

		public Dictionary<int, List<string>> GetForumViewRestrictionRoleGraph()
		{
			var cacheObject = _cacheHelper.GetCacheObject<Dictionary<int, List<string>>>(CacheKeys.ForumViewRoleRestrictions);
			if (cacheObject != null)
				return cacheObject;
			var dictionary = GetForumRestrictionRoleGraph("pf_ForumViewRestrictions");
			_cacheHelper.SetLongTermCacheObject(CacheKeys.ForumViewRoleRestrictions, dictionary);
			return dictionary;
		}

		private Dictionary<int, List<string>> GetForumRestrictionRoleGraph(string table)
		{
			var dictionary = new Dictionary<int, List<string>>();
			var forums = GetAll();
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

		private void ModifyForumRole(int forumID, string role, string sql)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute(sql, new { ForumID = forumID, Role = role }));
		}

		private void ModifyForumRole(int forumID, string sql)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute(sql, new { ForumID = forumID }));
		}

		public void AddPostRole(int forumID, string role)
		{
			RemovePostRole(forumID, role);
			ModifyForumRole(forumID, role, "INSERT INTO pf_ForumPostRestrictions (ForumID, Role) VALUES (@ForumID, @Role)");
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumPostRoleRestrictions);
		}

		public void RemovePostRole(int forumID, string role)
		{
			ModifyForumRole(forumID, role, "DELETE FROM pf_ForumPostRestrictions WHERE ForumID = @ForumID And Role = @Role");
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumPostRoleRestrictions);
		}

		public void AddViewRole(int forumID, string role)
		{
			RemoveViewRole(forumID, role);
			ModifyForumRole(forumID, role, "INSERT INTO pf_ForumViewRestrictions (ForumID, Role) VALUES (@ForumID, @Role)");
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumViewRoleRestrictions);
		}

		public void RemoveViewRole(int forumID, string role)
		{
			ModifyForumRole(forumID, role, "DELETE FROM pf_ForumViewRestrictions WHERE ForumID = @ForumID And Role = @Role");
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumViewRoleRestrictions);
		}

		public void RemoveAllPostRoles(int forumID)
		{
			ModifyForumRole(forumID, "DELETE FROM pf_ForumPostRestrictions WHERE ForumID = @ForumID");
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumPostRoleRestrictions);
		}

		public void RemoveAllViewRoles(int forumID)
		{
			ModifyForumRole(forumID, "DELETE FROM pf_ForumViewRestrictions WHERE ForumID = @ForumID");
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumViewRoleRestrictions);
		}

		public IEnumerable<string> GetAllForumUrlNames()
		{
			var cacheObject = _cacheHelper.GetCacheObject<IEnumerable<string>>(CacheKeys.ForumUrlNames);
			if (cacheObject != null)
				return cacheObject;
			IEnumerable<string> urlNames = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				urlNames = connection.Query<string>("SELECT UrlName FROM pf_Forum"));
			_cacheHelper.SetLongTermCacheObject(CacheKeys.ForumUrlNames, urlNames);
			return urlNames;
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

		public int GetAggregateTopicCount()
		{
			var cacheObject = _cacheHelper.GetCacheObject<int?>(CacheKeys.AggregateTopicCount);
			if (cacheObject != null)
				return cacheObject.Value;
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				count = connection.ExecuteScalar<int>("SELECT SUM(TopicCount) FROM pf_Forum"));
			_cacheHelper.SetCacheObject(CacheKeys.AggregateTopicCount, count);
			return count;
		}

		public int GetAggregatePostCount()
		{
			var cacheObject = _cacheHelper.GetCacheObject<int?>(CacheKeys.AggregatePostCount);
			if (cacheObject != null)
				return cacheObject.Value;
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				count = connection.ExecuteScalar<int>("SELECT SUM(PostCount) FROM pf_Forum"));
			_cacheHelper.SetCacheObject(CacheKeys.AggregatePostCount, count);
			return count;
		}
	}
}
