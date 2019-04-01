using Dapper;
using Newtonsoft.Json;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class SearchIndexQueueRepository : ISearchIndexQueueRepository
	{
		private readonly ISqlObjectFactory _sqlObjectFactory;

		public SearchIndexQueueRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		public void Enqueue(SearchIndexPayload payload)
		{
			var serializedPayload = JsonConvert.SerializeObject(payload);
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_SearchQueue (Payload) VALUES (@Payload)", new { Payload = serializedPayload }));
		}

		public SearchIndexPayload Dequeue()
		{
			string serializedPayload = null;
			var sql = @"WITH cte AS (
SELECT TOP(1) Payload
FROM pf_SearchQueue WITH (ROWLOCK, READPAST)
ORDER BY Id)
DELETE FROM cte
OUTPUT DELETED.Payload;";
			_sqlObjectFactory.GetConnection().Using(connection =>
				serializedPayload = connection.QuerySingleOrDefault<string>(sql));
			if (string.IsNullOrEmpty(serializedPayload))
				return new SearchIndexPayload();
			var payload = JsonConvert.DeserializeObject<SearchIndexPayload>(serializedPayload);
			return payload;
		}
	}
}