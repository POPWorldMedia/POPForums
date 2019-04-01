using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
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
pf_Topic.LastPostTime, pf_Topic.IsClosed, pf_Topic.IsPinned, pf_Topic.IsDeleted, pf_Topic.UrlName, pf_Topic.AnswerPostID 
FROM pf_Topic JOIN pf_Favorite F ON pf_Topic.TopicID = F.TopicID 
WHERE F.UserID = @UserID AND pf_Topic.IsDeleted = 0)

SELECT TopicID, ForumID, Title, ReplyCount, ViewCount, 
StartedByUserID, StartedByName, LastPostUserID, LastPostName, 
LastPostTime, IsClosed, IsPinned, IsDeleted, UrlName, AnswerPostID
FROM Entries 
WHERE Row between 
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<Topic>(sql, new { UserID = userID, StartRow = startRow, PageSize = pageSize }).ToList());
			return list;
		}

		public int GetFavoriteTopicCount(int userID)
		{
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM pf_Favorite F JOIN pf_Topic T ON F.TopicID = T.TopicID WHERE F.UserID = @UserID AND T.IsDeleted = 0", new { UserID = userID }));
			return count;
		}

		public bool IsTopicFavorite(int userID, int topicID)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.Query("SELECT * FROM pf_Favorite WHERE UserID = @UserID AND TopicID = @TopicID", new { UserID = userID, TopicID = topicID }).Any());
			return result;
		}

		public void AddFavoriteTopic(int userID, int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_Favorite (UserID, TopicID) VALUES (@UserID, @TopicID)", new { UserID = userID, TopicID = topicID }));
		}

		public void RemoveFavoriteTopic(int userID, int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_Favorite WHERE UserID = @UserID AND TopicID = @TopicID", new { UserID = userID, TopicID = topicID }));
		}
	}
}
