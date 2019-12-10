using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
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

		public async Task Enqueue(SearchIndexPayload payload)
		{
			var serializedPayload = JsonSerializer.Serialize(payload);
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("INSERT INTO pf_SearchQueue (Payload) VALUES (@Payload)", new { Payload = serializedPayload }));
		}

		public async Task<SearchIndexPayload> Dequeue()
		{
			var sql = @"WITH cte AS (
SELECT TOP(1) Payload
FROM pf_SearchQueue WITH (ROWLOCK, READPAST)
ORDER BY Id)
DELETE FROM cte
OUTPUT DELETED.Payload;";
			Task<string> serializedPayload = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				serializedPayload = connection.QuerySingleOrDefaultAsync<string>(sql));
			if (string.IsNullOrEmpty(serializedPayload.Result))
				return new SearchIndexPayload();
			var payload = JsonSerializer.Deserialize<SearchIndexPayload>(serializedPayload.Result);
			return payload;
		}
	}
}