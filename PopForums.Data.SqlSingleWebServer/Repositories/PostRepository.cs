using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
{
	public class PostRepository : IPostRepository
	{
		public PostRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;
		private const string _postFields = "PostID, TopicID, ParentPostID, IP, IsFirstInTopic, ShowSig, UserID, Name, Title, FullText, PostTime, IsEdited, LastEditName, LastEditTime, IsDeleted, Votes";

		public virtual int Create(int topicID, int parentPostID, string ip, bool isFirstInTopic, bool showSig, int userID, string name, string title, string fullText, DateTime postTime, bool isEdited, string lastEditName, DateTime? lastEditTime, bool isDeleted, int votes)
		{
			object postID = null;
			_sqlObjectFactory.GetConnection().Using(c => postID = c.Command("INSERT INTO pf_Post (TopicID, ParentPostID, IP, IsFirstInTopic, ShowSig, UserID, Name, Title, FullText, PostTime, IsEdited, LastEditName, LastEditTime, IsDeleted, Votes) VALUES (@TopicID, @ParentPostID, @IP, @IsFirstInTopic, @ShowSig, @UserID, @Name, @Title, @FullText, @PostTime, @IsEdited, @LastEditName, @LastEditTime, @IsDeleted, @Votes)")
				.AddParameter("@TopicID", topicID)
				.AddParameter("@ParentPostID", parentPostID)
				.AddParameter("@IP", ip)
				.AddParameter("@IsFirstInTopic", isFirstInTopic)
				.AddParameter("@ShowSig", showSig)
				.AddParameter("@UserID", userID)
				.AddParameter("@Name", name)
				.AddParameter("@Title", title)
				.AddParameter("@FullText", fullText)
				.AddParameter("@PostTime", postTime)
				.AddParameter("@IsEdited", isEdited)
				.AddParameter("@LastEditTime", lastEditTime.GetObjectOrDbNull())
				.AddParameter("@LastEditName", lastEditName)
				.AddParameter("@IsDeleted", isDeleted)
				.AddParameter("@Votes", votes)
				.ExecuteAndReturnIdentity());
			return Convert.ToInt32(postID);
		}

		public bool Update(Post post)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(c => result = c.Command("UPDATE pf_Post SET TopicID = @TopicID, ParentPostID = @ParentPostID, IP = @IP, IsFirstInTopic = @IsFirstInTopic, ShowSig = @ShowSig, UserID = @UserID, Name = @Name, Title = @Title, FullText = @FullText, PostTime = @PostTime, IsEdited = @IsEdited, LastEditName = @LastEditName, LastEditTime = @LastEditTime, IsDeleted = @IsDeleted, Votes = @Votes WHERE PostID = @PostID")
				.AddParameter("@TopicID", post.TopicID)
				.AddParameter("@ParentPostID", post.ParentPostID)
				.AddParameter("@IP", post.IP)
				.AddParameter("@IsFirstInTopic", post.IsFirstInTopic)
				.AddParameter("@ShowSig", post.ShowSig)
				.AddParameter("@UserID", post.UserID)
				.AddParameter("@Name", post.Name)
				.AddParameter("@Title", post.Title)
				.AddParameter("@FullText", post.FullText)
				.AddParameter("@PostTime", post.PostTime)
				.AddParameter("@IsEdited", post.IsEdited)
				.AddParameter("@LastEditTime", post.LastEditTime.GetObjectOrDbNull())
				.AddParameter("@LastEditName", post.LastEditName)
				.AddParameter("@IsDeleted", post.IsDeleted)
				.AddParameter("@PostID", post.PostID)
				.AddParameter("@Votes", post.Votes)
				.ExecuteNonQuery() == 1);
			return result;
		}

		public List<Post> Get(int topicID, bool includeDeleted, int startRow, int pageSize)
		{
			const string sql = @"
DECLARE @Counter int
SET @Counter = (@StartRow + @PageSize - 1)

SET ROWCOUNT @Counter;

WITH Entries AS ( 
SELECT ROW_NUMBER() OVER (ORDER BY PostTime)
AS Row, PostID, TopicID, ParentPostID, IP, IsFirstInTopic, ShowSig, UserID, Name, Title, FullText, PostTime, IsEdited, LastEditName, LastEditTime, IsDeleted, Votes 
FROM pf_Post WHERE TopicID = @TopicID 
AND ((@IncludeDeleted = 1) OR (@IncludeDeleted = 0 AND IsDeleted = 0)))

SELECT PostID, TopicID, ParentPostID, IP, IsFirstInTopic, ShowSig, UserID, Name, Title, FullText, PostTime, IsEdited, LastEditName, LastEditTime, IsDeleted, Votes
FROM Entries 
WHERE Row between 
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			var posts = new List<Post>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(sql)
					.AddParameter("@TopicID", topicID)
					.AddParameter("@IncludeDeleted", includeDeleted)
					.AddParameter("@StartRow", startRow)
					.AddParameter("@PageSize", pageSize)
					.ExecuteReader()
					.ReadAll(r => posts.Add(GetPostFromReader(r))));
			return posts;
		}

		public List<Post> Get(int topicID, bool includeDeleted)
		{
			const string sql = "SELECT PostID, TopicID, ParentPostID, IP, IsFirstInTopic, ShowSig, UserID, Name, Title, FullText, PostTime, IsEdited, LastEditName, LastEditTime, IsDeleted, Votes FROM pf_Post WHERE TopicID = @TopicID AND ((@IncludeDeleted = 1) OR (@IncludeDeleted = 0 AND IsDeleted = 0)) ORDER BY PostTime";
			var posts = new List<Post>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(sql)
					.AddParameter("@TopicID", topicID)
					.AddParameter("@IncludeDeleted", includeDeleted)
					.ExecuteReader()
					.ReadAll(r => posts.Add(GetPostFromReader(r))));
			return posts;
		}

		public List<Post> GetPostWithRepies(int postID, bool includeDeleted)
		{
			const string sql = "SELECT PostID, TopicID, ParentPostID, IP, IsFirstInTopic, ShowSig, UserID, Name, Title, FullText, PostTime, IsEdited, LastEditName, LastEditTime, IsDeleted, Votes FROM pf_Post WHERE (PostID = @PostID OR ParentPostID = @PostID) AND ((@IncludeDeleted = 1) OR (@IncludeDeleted = 0 AND IsDeleted = 0)) ORDER BY PostTime";
			var posts = new List<Post>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(sql)
					.AddParameter("@PostID", postID)
					.AddParameter("@IncludeDeleted", includeDeleted)
					.ExecuteReader()
					.ReadAll(r => posts.Add(GetPostFromReader(r))));
			return posts;
		}

		public Post GetFirstInTopic(int topicID)
		{
			Post post = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT " + _postFields + " FROM pf_Post WHERE TopicID = @TopicID AND IsFirstInTopic = 1")
					.AddParameter("@TopicID", topicID)
					.ExecuteReader()
					.ReadOne(r => post = GetPostFromReader(r)));
			return post;
		}

		public Post GetLastInTopic(int topicID)
		{
			Post post = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT TOP 1 " + _postFields + " FROM pf_Post WHERE TopicID = @TopicID AND IsDeleted = 0 ORDER BY PostTime DESC")
					.AddParameter("@TopicID", topicID)
					.ExecuteReader()
					.ReadOne(r => post = GetPostFromReader(r)));
			return post;
		}

		public int GetReplyCount(int topicID, bool includeDeleted)
		{
			var sql = "SELECT COUNT(*) FROM pf_Post WHERE TopicID = @TopicID";
			if (!includeDeleted)
				sql += " AND IsDeleted = 0 AND IsFirstInTopic = 0";
			var replyCount = 0;
			_sqlObjectFactory.GetConnection().Using(c =>
				replyCount = Convert.ToInt32(c.Command(sql)
				.AddParameter("@TopicID", topicID)
				.ExecuteScalar()));
			return replyCount;
		}

		public Post Get(int postID)
		{
			Post post = null;
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command("SELECT " + _postFields + " FROM pf_Post WHERE PostID = @PostID")
				.AddParameter("@PostID", postID)
				.ExecuteReader()
				.ReadOne(r => post = GetPostFromReader(r)));
			return post;
		}

		public Dictionary<int, DateTime> GetPostIDsWithTimes(int topicID, bool includeDeleted)
		{
			var dictionary = new Dictionary<int, DateTime>();
			var sql = "SELECT PostID, PostTime FROM pf_Post WHERE TopicID = @TopicID";
			if (!includeDeleted)
				sql += " AND IsDeleted = 0";
			sql += " ORDER BY PostTime";
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command(sql)
				.AddParameter("@TopicID", topicID)
				.ExecuteReader()
				.ReadAll(r => dictionary.Add(r.GetInt32(0), r.GetDateTime(1))));
			return dictionary;
		}

		public int GetPostCount(int userID)
		{
			var postCount = 0;
			_sqlObjectFactory.GetConnection().Using(c =>
				postCount = Convert.ToInt32(c.Command("SELECT COUNT(PostID) FROM pf_Post JOIN pf_Topic ON pf_Post.TopicID = pf_Topic.TopicID WHERE pf_Post.UserID = @UserID AND pf_Post.IsDeleted = 0 AND pf_Topic.IsDeleted = 0")
				.AddParameter("@UserID", userID)
				.ExecuteScalar()));
			return postCount;
		}

		public List<IPHistoryEvent> GetIPHistory(string ip, DateTime start, DateTime end)
		{
			var list = new List<IPHistoryEvent>();
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command("SELECT PostID, PostTime, UserID, Name, Title FROM pf_Post WHERE IP = @IP AND PostTime >= @Start AND PostTime <= @End")
				.AddParameter("@IP", ip)
				.AddParameter("@Start", start)
				.AddParameter("@End", end)
				.ExecuteReader()
				.ReadAll(r => list.Add(new IPHistoryEvent
				                       	{
				                       		ID = r.GetInt32(0),
											EventTime = r.GetDateTime(1),
											UserID = r.GetInt32(2),
											Name = r.GetString(3),
											Description = r.GetString(4),
											Type = typeof(Post)
				                       	})));
			return list;
		}

		public Dictionary<int, int> GetFirstPostIDsFromTopicIDs(List<int> topicIDs)
		{
			var ids = String.Join(",", topicIDs);
			var sql = "SELECT TopicID, PostID FROM pf_Post WHERE IsFirstInTopic = 1 AND TopicID IN (" + ids + ")";
			var dictionary = new Dictionary<int, int>();
			_sqlObjectFactory.GetConnection().Using(c =>
			    c.Command(sql)
			    .ExecuteReader()
			    .ReadAll(r => dictionary.Add(r.GetInt32(0), r.GetInt32(1))));
			return dictionary;
		}

		public int GetLastPostID(int topicID)
		{
			const string sql = "SELECT PostID FROM pf_Post WHERE TopicID = @TopicID AND IsDeleted = 0 ORDER BY PostTime DESC";
			var id = 0;
			_sqlObjectFactory.GetConnection().Using(c => id = Convert.ToInt32(
				c.Command(sql)
				.AddParameter("@TopicID", topicID)
				.ExecuteScalar()));
			return id;
		}

		public int GetVoteCount(int postID)
		{
			const string sql = "SELECT Votes FROM pf_Post WHERE PostID = @PostID";
			var votes = 0;
			_sqlObjectFactory.GetConnection().Using(c => votes = Convert.ToInt32(
				c.Command(sql)
				.AddParameter("@PostID", postID)
				.ExecuteScalar()));
			return votes;
		}

		public int CalculateVoteCount(int postID)
		{
			const string sql = "SELECT COUNT(*) FROM pf_PostVote WHERE PostID = @PostID";
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(c => count = Convert.ToInt32(
				c.Command(sql)
				.AddParameter("@PostID", postID)
				.ExecuteScalar()));
			return count;
		}

		public void SetVoteCount(int postID, int votes)
		{
			const string sql = "UPDATE pf_Post SET Votes = @Votes WHERE PostID = @PostID";
			_sqlObjectFactory.GetConnection().Using(c => 
				c.Command(sql)
				.AddParameter("@Votes", votes)
				.AddParameter("@PostID", postID)
				.ExecuteNonQuery());
		}

		public void VotePost(int postID, int userID)
		{
			const string sql = "INSERT INTO pf_PostVote (PostID, UserID) VALUES (@PostID, @UserID)";
			_sqlObjectFactory.GetConnection().Using(c => 
				c.Command(sql)
				.AddParameter("@PostID", postID)
				.AddParameter("@UserID", userID)
				.ExecuteNonQuery());
		}

		public Dictionary<int, string> GetVotes(int postID)
		{
			var results = new Dictionary<int, string>();
			const string sql = "SELECT V.UserID, U.Name FROM pf_PostVote V LEFT JOIN pf_PopForumsUser U ON V.UserID = U.UserID WHERE V.PostID = @PostID";
			_sqlObjectFactory.GetConnection().Using(c =>
				c.Command(sql)
				.AddParameter("@PostID", postID)
				.ExecuteReader()
				.ReadAll(r => results.Add(r.GetInt32(0), r.NullStringDbHelper(1))));
			return results;
		}

		public List<int> GetVotedPostIDs(int userID, List<int> postIDs)
		{
			var list = new List<int>();
			if (postIDs.Count == 0)
				return list;
			var inList = postIDs.Aggregate(String.Empty, (current, postID) => current + ("," + postID));
			if (inList.StartsWith(","))
				inList = inList.Remove(0, 1);
			var sql = String.Format("SELECT PostID FROM pf_PostVote WHERE PostID IN ({0}) AND UserID = @UserID", inList);
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(sql)
					.AddParameter("@UserID", userID)
					.ExecuteReader()
					.ReadAll(r => list.Add(r.GetInt32(0))));
			return list;
		}

		private static Post GetPostFromReader(IDataRecord r)
		{
			return new Post(r.GetInt32(0))
			       	{
			       		TopicID = r.GetInt32(1),
						ParentPostID = r.GetInt32(2),
						IP = r.GetString(3),
						IsFirstInTopic = r.GetBoolean(4),
						ShowSig = r.GetBoolean(5),
						UserID = r.GetInt32(6),
						Name = r.GetString(7),
						Title = r.GetString(8),
						FullText = r.GetString(9),
						PostTime = r.GetDateTime(10),
						IsEdited = r.GetBoolean(11),
						LastEditName = r.GetString(12),
						LastEditTime = r.NullDateTimeDbHelper(13),
						IsDeleted = r.GetBoolean(14),
						Votes = r.GetInt32(15)
			       	};
		}
	}
}
