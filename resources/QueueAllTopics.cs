#:package Microsoft.Data.SqlClient@7.1.1
#:package Azure.Storage.Queues@12.28.1
#:property PublishAot=false

// Queues every non-deleted topic in one POP Forums database for search reindexing.
// Run once per site:
//   dotnet run QueueAllTopics.cs -- "<sql connection string>" "<storage connection string>" "<tenant id>"
// Tenant ID is optional; omit it (or pass "") for a single-tenant site, matching TenantService.GetTenant().

using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Data.SqlClient;

if (args.Length < 2)
{
	Console.Error.WriteLine("Usage: dotnet run QueueAllTopics.cs -- <sqlConnectionString> <storageConnectionString> [tenantID]");
	return 1;
}
var sqlConnectionString = args[0];
var storageConnectionString = args[1];
var tenantID = args.Length > 2 ? args[2] : string.Empty;

// Deleted topics are skipped: the ElasticSearch query doesn't filter on IsDeleted.
var topicIDs = new List<int>();
await using (var connection = new SqlConnection(sqlConnectionString))
{
	await connection.OpenAsync();
	await using var command = new SqlCommand("SELECT TopicID FROM pf_Topic WHERE IsDeleted = 0 ORDER BY TopicID", connection);
	await using var reader = await command.ExecuteReaderAsync();
	while (await reader.ReadAsync())
		topicIDs.Add(reader.GetInt32(0));
}
Console.WriteLine($"Found {topicIDs.Count} topics for tenant \"{tenantID}\".");

// Same queue name and encoding as PopForums.AzureKit.Queue.SearchIndexQueueRepository.
var queue = new QueueClient(storageConnectionString, "pfsearchindexqueue",
	new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
await queue.CreateIfNotExistsAsync();

var sent = 0;
var failed = new List<int>();
await Parallel.ForEachAsync(topicIDs, new ParallelOptions { MaxDegreeOfParallelism = 32 }, async (topicID, ct) =>
{
	// Same shape and property names as PopForums.Models.SearchIndexPayload.
	var payload = JsonSerializer.Serialize(new { TopicID = topicID, TenantID = tenantID, IsForRemoval = false });
	try
	{
		await queue.SendMessageAsync(payload, ct);
		var count = Interlocked.Increment(ref sent);
		if (count % 1000 == 0)
			Console.WriteLine($"Queued {count}...");
	}
	catch (Exception exc)
	{
		lock (failed)
			failed.Add(topicID);
		Console.Error.WriteLine($"TopicID {topicID} failed: {exc.Message}");
	}
});

Console.WriteLine($"Queued {sent} of {topicIDs.Count}.");
if (failed.Count > 0)
{
	Console.Error.WriteLine($"Failed TopicIDs: {string.Join(",", failed.OrderBy(x => x))}");
	return 2;
}
return 0;
