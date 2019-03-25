using System;
using System.Threading.Tasks;
using Dapper;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class TopicViewLogRepository : ITopicViewLogRepository
	{
		private readonly ISqlObjectFactory _sqlObjectFactory;

		public TopicViewLogRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		public async Task Log(int? userID, int topicID, DateTime timeStamp)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_TopicViewLog (UserID, TopicID, [TimeStamp]) VALUES (@UserID, @TopicID, @TimeStamp)", new {UserID = userID, TopicID = topicID, TimeStamp = timeStamp}));
		}
	}
}