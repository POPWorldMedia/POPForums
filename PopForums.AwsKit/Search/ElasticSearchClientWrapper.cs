using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using PopForums.Configuration;
using PopForums.Models;

namespace PopForums.AwsKit.Search
{
	public interface IElasticSearchClientWrapper
	{
		IIndexResponse IndexTopic(SearchTopic searchTopic);
		IEnumerable<int> SearchTopicsWithIDs(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize, out int topicCount);
	}

	public class ElasticSearchClientWrapper : IElasticSearchClientWrapper
	{
		private readonly ElasticClient _client;

		private const string IndexName = "topicindex";

		public ElasticSearchClientWrapper(IConfig config)
		{
			var node = new Uri(config.SearchUrl);
			var settings = new ConnectionSettings(node)
				.DefaultIndex(IndexName);
			_client = new ElasticClient(settings);
		}

		public IIndexResponse IndexTopic(SearchTopic searchTopic)
		{
			var indexResult = _client.IndexDocument(searchTopic);

			return indexResult;
		}

		public IEnumerable<int> SearchTopicsWithIDs(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize, out int topicCount)
		{
			startRow--;
			var searchResponse = _client.Search<SearchTopic>(s => s
				.Source(sf => sf.Includes(i => i.Fields(f => f.Id)))
				.Query(q => q.MatchAll())
				.Take(pageSize)
				.Skip(startRow));
			var ids = searchResponse.Documents.Select(d => Convert.ToInt32(d.Id));
			topicCount = (int)searchResponse.Total;
			return ids;
		}
	}
}