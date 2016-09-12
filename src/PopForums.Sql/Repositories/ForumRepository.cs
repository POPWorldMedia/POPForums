using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.Sql.Repositories
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

		private static Forum PopulateForumFromReader(DbDataReader r)
		{
			return new Forum(r.GetInt32(0))
			       	{
			       		CategoryID = r.NullIntDbHelper(1),
			       		Title = r.GetString(2),
			       		Description = r.GetString(3),
			       		IsVisible = r.GetBoolean(4),
			       		IsArchived = r.GetBoolean(5),
			       		SortOrder = r.GetInt32(6),
			       		TopicCount = r.GetInt32(7),
			       		PostCount = r.GetInt32(8),
			       		LastPostTime = r.GetDateTime(9),
			       		LastPostName = r.GetString(10),
			       		UrlName = r.GetString(11),
						ForumAdapterName = r.NullStringDbHelper(12),
						IsQAForum = r.GetBoolean(13)
			       	};
		}

		public Forum Get(int forumID)
		{
			Forum forum = null;
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command("SELECT " + ForumFields + " FROM pf_Forum WHERE ForumID = @ForumID")
					.AddParameter("@ForumID", forumID)
					.ExecuteReader()
					.ReadOne(r => forum = PopulateForumFromReader(r)));
			return forum;
		}

		public Forum Get(string urlName)
		{
			Forum forum = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command("SELECT " + ForumFields + " FROM pf_Forum WHERE UrlName = @UrlName")
					.AddParameter("@UrlName", urlName)
					.ExecuteReader()
					.ReadOne(r => forum = PopulateForumFromReader(r)));
			return forum;
		}

		public Forum Create(int? categoryID, string title, string description, bool isVisible, bool isArchived, int sortOrder, string urlName, string forumAdapterName, bool isQAForum)
		{
			if (categoryID == 0)
				categoryID = null;
			var forumID = 0;
			var lastPostTime = new DateTime(2000, 1, 1);
			_sqlObjectFactory.GetConnection().Using(connection => forumID = Convert.ToInt32(
				connection.Command("INSERT INTO pf_Forum (CategoryID, Title, Description, IsVisible, IsArchived, SortOrder, TopicCount, PostCount, LastPostTime, LastPostName, UrlName, ForumAdapterName, IsQAForum) VALUES (@CategoryID, @Title, @Description, @IsVisible, @IsArchived, @SortOrder, 0, 0, @LastPostTime, '', @UrlName, @ForumAdapterName, @IsQAForum)")
				.AddParameter("@CategoryID", categoryID.GetObjectOrDbNull())
				.AddParameter("@Title", title)
				.AddParameter("@Description", description.NullToEmpty())
				.AddParameter("@IsVisible", isVisible)
				.AddParameter("@IsArchived", isArchived)
				.AddParameter("@SortOrder", sortOrder)
				.AddParameter("@LastPostTime", lastPostTime)
				.AddParameter("@UrlName", urlName)
				.AddParameter("@ForumAdapterName", forumAdapterName.GetObjectOrDbNull())
				.AddParameter("@IsQAForum", isQAForum)
				.ExecuteAndReturnIdentity()));
			var forum = new Forum(forumID)
			            	{
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
					connection.Command("SELECT " + ForumFields + " FROM pf_Forum WHERE CategoryID = @CategoryID")
						.AddParameter("@CategoryID", categoryID.Value)
						.ExecuteReader()
						.ReadAll(r => forums.Add(PopulateForumFromReader(r))));
			else
				_sqlObjectFactory.GetConnection().Using(connection =>
					connection.Command("SELECT " + ForumFields + " FROM pf_Forum WHERE CategoryID = 0 OR CategoryID IS NULL")
						.ExecuteReader()
						.ReadAll(r => forums.Add(PopulateForumFromReader(r))));
			return forums;
		}

		public List<string> GetUrlNamesThatStartWith(string urlName)
		{
			var list = new List<string>();
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command("SELECT UrlName FROM pf_Forum WHERE UrlName LIKE @UrlName + '%'")
				.AddParameter("@UrlName", urlName)
				.ExecuteReader()
				.ReadAll(r => list.Add(r.GetString(0))));
			return list;
		}

		public void Update(int forumID, int? categoryID, string title, string description, bool isVisible, bool isArchived, string urlName, string forumAdapterName, bool isQAForum)
		{
			if (categoryID == 0)
				categoryID = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("UPDATE pf_Forum SET CategoryID = @CategoryID, Title = @Title, Description = @Description, IsVisible = @IsVisible, IsArchived = @IsArchived, UrlName = @UrlName, ForumAdapterName = @ForumAdapterName, IsQAForum = @IsQAForum WHERE ForumID = @ForumID")
				.AddParameter("@CategoryID", categoryID.GetObjectOrDbNull())
				.AddParameter("@Title", title)
				.AddParameter("@Description", description.NullToEmpty())
				.AddParameter("@IsVisible", isVisible)
				.AddParameter("@IsArchived", isArchived)
				.AddParameter("@UrlName", urlName)
				.AddParameter("@ForumID", forumID)
				.AddParameter("@ForumAdapterName", forumAdapterName.GetObjectOrDbNull())
				.AddParameter("@IsQAForum", isQAForum)
				.ExecuteNonQuery());
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumUrlNames);
			_cacheHelper.RemoveCacheObject(CacheKeys.ForumTitles);
		}

		public void UpdateSortOrder(int forumID, int newSortOrder)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command("UPDATE pf_Forum SET SortOrder = @SortOrder WHERE ForumID = @ForumID")
				.AddParameter("@SortOrder", newSortOrder)
				.AddParameter("@ForumID", forumID)
				.ExecuteNonQuery());
		}

		public void UpdateCategoryAssociation(int forumID, int? categoryID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command("UPDATE pf_Forum SET CategoryID = @CategoryID WHERE ForumID = @ForumID")
				.AddParameter("@CategoryID", categoryID.GetObjectOrDbNull())
				.AddParameter("@ForumID", forumID)
				.ExecuteNonQuery());
		}

		public void UpdateLastTimeAndUser(int forumID, DateTime lastTime, string lastName)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command("UPDATE pf_Forum SET LastPostTime = @LastPostTime, LastPostName = @LastPostName WHERE ForumID = @ForumID")
				.AddParameter("@LastPostTime", lastTime)
				.AddParameter("@LastPostName", lastName)
				.AddParameter("@ForumID", forumID)
				.ExecuteNonQuery());
		}

		public void UpdateTopicAndPostCounts(int forumID, int topicCount, int postCount)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command("UPDATE pf_Forum SET TopicCount = @TopicCount, PostCount = @PostCount WHERE ForumID = @ForumID")
				.AddParameter("@TopicCount", topicCount)
				.AddParameter("@PostCount", postCount)
				.AddParameter("@ForumID", forumID)
				.ExecuteNonQuery());
		}

		public void IncrementPostCount(int forumID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command("UPDATE pf_Forum SET PostCount = PostCount + 1 WHERE ForumID = @ForumID")
				.AddParameter("@ForumID", forumID)
				.ExecuteNonQuery());
		}

		public void IncrementPostAndTopicCount(int forumID)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command("UPDATE pf_Forum SET TopicCount = TopicCount + 1, PostCount = PostCount + 1 WHERE ForumID = @ForumID")
				.AddParameter("@ForumID", forumID)
				.ExecuteNonQuery());
		}

		public IEnumerable<Forum> GetAll()
		{
			var forums = new List<Forum>();
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command("SELECT " + ForumFields + " FROM pf_Forum ORDER BY SortOrder")
			        .ExecuteReader()
			        .ReadAll(r => forums.Add(PopulateForumFromReader(r))));
			return forums;
		}

		public IEnumerable<Forum> GetAllVisible()
		{
			var forums = new List<Forum>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT " + ForumFields + " FROM pf_Forum WHERE IsVisible = 1 ORDER BY SortOrder")
					.ExecuteReader()
					.ReadAll(r => forums.Add(PopulateForumFromReader(r))));
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
			_cacheHelper.SetCacheObject(CacheKeys.ForumPostRoleRestrictions, dictionary);
			return dictionary;
		}

		public Dictionary<int, List<string>> GetForumViewRestrictionRoleGraph()
		{
			var cacheObject = _cacheHelper.GetCacheObject<Dictionary<int, List<string>>>(CacheKeys.ForumViewRoleRestrictions);
			if (cacheObject != null)
				return cacheObject;
			var dictionary = GetForumRestrictionRoleGraph("pf_ForumViewRestrictions");
			_cacheHelper.SetCacheObject(CacheKeys.ForumViewRoleRestrictions, dictionary);
			return dictionary;
		}

		private Dictionary<int, List<string>> GetForumRestrictionRoleGraph(string table)
		{
			var dictionary = new Dictionary<int, List<string>>();
			var forums = GetAll();
			foreach (var forum in forums)
				dictionary.Add(forum.ForumID, new List<string>());
			_sqlObjectFactory.GetConnection().Using(connection =>
											 connection.Command("SELECT ForumID, Role FROM " + table)
												.ExecuteReader()
												.ReadAll(r => 
													dictionary.Single(d => d.Key == r.GetInt32(0)).Value
														.Add(r.GetString(1))
												));
			return dictionary;
		}

		private void ModifyForumRole(int forumID, string role, string sql)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(sql)
				.AddParameter("@ForumID", forumID)
				.AddParameter("@Role", role)
				.ExecuteNonQuery());
		}

		private void ModifyForumRole(int forumID, string sql)
		{
			_sqlObjectFactory.GetConnection().Using(connection => connection.Command(sql)
				.AddParameter("@ForumID", forumID)
				.ExecuteNonQuery());
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
			var urlNames = new List<string>();
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command("SELECT UrlName FROM pf_Forum")
			        .ExecuteReader()
			        .ReadAll(r => urlNames.Add(r.GetString(0))));
			_cacheHelper.SetCacheObject(CacheKeys.ForumUrlNames, urlNames);
			return urlNames;
		}

		public Dictionary<int, string> GetAllForumTitles()
		{
			var cacheObject = _cacheHelper.GetCacheObject<Dictionary<int, string>>(CacheKeys.ForumTitles);
			if (cacheObject != null)
				return cacheObject;
			var urlNames = new Dictionary<int, string>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT ForumID, Title FROM pf_Forum")
					.ExecuteReader()
					.ReadAll(r => urlNames.Add(r.GetInt32(0), r.GetString(1))));
			_cacheHelper.SetCacheObject(CacheKeys.ForumTitles, urlNames);
			return urlNames;
		}

		public int GetAggregateTopicCount()
		{
			var cacheObject = _cacheHelper.GetCacheObject<int?>(CacheKeys.AggregateTopicCount);
			if (cacheObject != null)
				return cacheObject.Value;
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
			                                 	{
			                                 		var result = connection.Command("SELECT SUM(TopicCount) FROM pf_Forum")
														.ExecuteScalar();
													if (result.GetType() == typeof(Int32))
														count = Convert.ToInt32(result);
			                                 	});
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
			                                 	{
			                                 		var result = connection.Command("SELECT SUM(PostCount) FROM pf_Forum")
														.ExecuteScalar();
													if (result.GetType() == typeof(Int32))
														count = Convert.ToInt32(result);
			                                 	});
			_cacheHelper.SetCacheObject(CacheKeys.AggregatePostCount, count);
			return count;
		}
	}
}
