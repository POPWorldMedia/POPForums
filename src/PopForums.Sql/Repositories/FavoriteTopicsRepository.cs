using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		public async Task<List<Topic>> GetFavoriteTopics(int userID, int startRow, int pageSize)
		{
			Task<IEnumerable<Topic>> result = null;
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
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync<Topic>(sql, new { UserID = userID, StartRow = startRow, PageSize = pageSize }));
			return result.Result.ToList();
		}

		public async Task<int> GetFavoriteTopicCount(int userID)
		{
			Task<int> count = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				count = connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM pf_Favorite F JOIN pf_Topic T ON F.TopicID = T.TopicID WHERE F.UserID = @UserID AND T.IsDeleted = 0", new { UserID = userID }));
			return await count;
		}

		public async Task<bool> IsTopicFavorite(int userID, int topicID)
		{
			Task<IEnumerable<dynamic>> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QueryAsync("SELECT * FROM pf_Favorite WHERE UserID = @UserID AND TopicID = @TopicID", new { UserID = userID, TopicID = topicID }));
			return result.Result.Any();
		}

		public async Task AddFavoriteTopic(int userID, int topicID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_Favorite (UserID, TopicID) VALUES (@UserID, @TopicID)", new { UserID = userID, TopicID = topicID }));
		}

		public async Task RemoveFavoriteTopic(int userID, int topicID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_Favorite WHERE UserID = @UserID AND TopicID = @TopicID", new { UserID = userID, TopicID = topicID }));
		}
	}
}
