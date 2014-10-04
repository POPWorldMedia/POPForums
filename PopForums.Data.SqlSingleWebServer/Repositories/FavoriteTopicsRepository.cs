using System;
using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
{
	public class FavoriteTopicsRepository : IFavoriteTopicsRepository
	{
		public FavoriteTopicsRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public List<Topic> GetFavoriteTopics(int userID, int startRow, int pageSize)
		{
			var list = new List<Topic>();
			const string sql = @"
DECLARE @Counter int
SET @Counter = (@StartRow + @PageSize - 1)

SET ROWCOUNT @Counter;

WITH Entries AS ( 
SELECT ROW_NUMBER() OVER (ORDER BY IsPinned DESC, LastPostTime DESC)
AS Row, pf_Topic.TopicID, pf_Topic.ForumID, pf_Topic.Title, pf_Topic.ReplyCount, pf_Topic.ViewCount, 
pf_Topic.StartedByUserID, pf_Topic.StartedByName, pf_Topic.LastPostUserID, pf_Topic.LastPostName, 
pf_Topic.LastPostTime, pf_Topic.IsClosed, pf_Topic.IsPinned, pf_Topic.IsDeleted, pf_Topic.IsIndexed, pf_Topic.UrlName, pf_Topic.AnswerPostID 
FROM pf_Topic JOIN pf_Favorite F ON pf_Topic.TopicID = F.TopicID 
WHERE F.UserID = @UserID AND pf_Topic.IsDeleted = 0)

SELECT TopicID, ForumID, Title, ReplyCount, ViewCount, 
StartedByUserID, StartedByName, LastPostUserID, LastPostName, 
LastPostTime, IsClosed, IsPinned, IsDeleted, IsIndexed, UrlName, AnswerPostID
FROM Entries 
WHERE Row between 
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(sql)
				.AddParameter("@UserID", userID)
				.AddParameter("@StartRow", startRow)
				.AddParameter("@PageSize", pageSize)
				.ExecuteReader()
				.ReadAll(r => list.Add(TopicRepository.GetTopicFromReader(r))));
			return list;
		}

		public int GetFavoriteTopicCount(int userID)
		{
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				count = Convert.ToInt32(connection.Command("SELECT COUNT(*) FROM pf_Favorite F JOIN pf_Topic T ON F.TopicID = T.TopicID WHERE F.UserID = @UserID AND T.IsDeleted = 0")
				.AddParameter("@UserID", userID)
				.ExecuteScalar()));
			return count;
		}

		public bool IsTopicFavorite(int userID, int topicID)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT * FROM pf_Favorite WHERE UserID = @UserID AND TopicID = @TopicID")
				.AddParameter("@UserID", userID)
				.AddParameter("@TopicID", topicID)
				.ExecuteReader()
				.ReadOne(r => result = true));
			return result;
		}

		public void AddFavoriteTopic(int userID, int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("INSERT INTO pf_Favorite (UserID, TopicID) VALUES (@UserID, @TopicID)")
				.AddParameter("@UserID", userID)
				.AddParameter("@TopicID", topicID)
				.ExecuteNonQuery());
		}

		public void RemoveFavoriteTopic(int userID, int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_Favorite WHERE UserID = @UserID AND TopicID = @TopicID")
				.AddParameter("@UserID", userID)
				.AddParameter("@TopicID", topicID)
				.ExecuteNonQuery());
		}
	}
}
