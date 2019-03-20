using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class SubscribedTopicsRepository : ISubscribedTopicsRepository
	{
		public SubscribedTopicsRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public List<Topic> GetSubscribedTopics(int userID, int startRow, int pageSize)
		{
			List<Topic> list = null;
			const string sql = @"
DECLARE @Counter int
SET @Counter = (@StartRow + @PageSize - 1)

SET ROWCOUNT @Counter;

WITH Entries AS ( 
SELECT ROW_NUMBER() OVER (ORDER BY IsPinned DESC, LastPostTime DESC)
AS Row, pf_Topic.TopicID, pf_Topic.ForumID, pf_Topic.Title, pf_Topic.ReplyCount, pf_Topic.ViewCount, 
pf_Topic.StartedByUserID, pf_Topic.StartedByName, pf_Topic.LastPostUserID, pf_Topic.LastPostName, 
pf_Topic.LastPostTime, pf_Topic.IsClosed, pf_Topic.IsPinned, pf_Topic.IsDeleted, pf_Topic.UrlName, pf_Topic.AnswerPostID 
FROM pf_Topic JOIN pf_SubscribeTopic S ON pf_Topic.TopicID = S.TopicID 
WHERE S.UserID = @UserID AND pf_Topic.IsDeleted = 0)

SELECT TopicID, ForumID, Title, ReplyCount, ViewCount, 
StartedByUserID, StartedByName, LastPostUserID, LastPostName, 
LastPostTime, IsClosed, IsPinned, IsDeleted, UrlName, AnswerPostID
FROM Entries 
WHERE Row between 
@StartRow and @StartRow + @PageSize - 1

SET ROWCOUNT 0";
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<Topic>(sql, new { UserID = userID, StartRow = startRow, PageSize = pageSize}).ToList());
			return list;
		}

		public int GetSubscribedTopicCount(int userID)
		{
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM pf_SubscribeTopic S JOIN pf_Topic T ON S.TopicID = T.TopicID WHERE S.UserID = @UserID AND T.IsDeleted = 0", new { UserID = userID }));
			return count;
		}

		public List<User> GetSubscribedUsersThatHaveViewed(int topicID)
		{
			List<User> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<User>("SELECT " + UserRepository.PopForumsUserColumns + " FROM pf_PopForumsUser JOIN pf_SubscribeTopic ON pf_PopForumsUser.UserID = pf_SubscribeTopic.UserID WHERE TopicID = @TopicID AND IsViewed = 1", new { TopicID = topicID }).ToList());
			return list;
		}

		public bool IsTopicSubscribed(int userID, int topicID)
		{
			var result = false;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.Query("SELECT * FROM pf_SubscribeTopic WHERE UserID = @UserID AND TopicID = @TopicID", new { UserID = userID, TopicID = topicID }).Any());
			return result;
		}

		public void AddSubscribedTopic(int userID, int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_SubscribeTopic (UserID, TopicID, IsViewed) VALUES (@UserID, @TopicID, 1)", new { UserID = userID, TopicID = topicID }));
		}

		public void RemoveSubscribedTopic(int userID, int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_SubscribeTopic WHERE UserID = @UserID AND TopicID = @TopicID", new { UserID = userID, TopicID = topicID }));
		}

		public void MarkSubscribedTopicViewed(int userID, int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("UPDATE pf_SubscribeTopic SET IsViewed = 1 WHERE UserID = @UserID AND TopicID = @TopicID", new { UserID = userID, TopicID = topicID }));
		}

		public void MarkSubscribedTopicUnviewed(int topicID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("UPDATE pf_SubscribeTopic SET IsViewed = 0 WHERE TopicID = @TopicID", new { TopicID = topicID }));
		}
	}
}
