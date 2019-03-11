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
		void VerifyIndexCreate();
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
			// filter hidden forums
			// sort order
			startRow--;
			var searchResponse = _client.Search<SearchTopic>(s => s
				.Source(sf => sf.Includes(i => i.Fields(f => f.Id)))
				.Query(q => q.MultiMatch(m => m.Query(searchTerm)
					.Fields(f => f
						.Field(x => x.Title, boost: 20)
						.Field(x => x.FirstPost, boost: 2)
						.Field(x => x.Posts))))
				.Take(pageSize)
				.Skip(startRow));
			var ids = searchResponse.Documents.Select(d => Convert.ToInt32(d.Id));
			topicCount = (int)searchResponse.Total;
			return ids;
		}

		public void VerifyIndexCreate()
		{
			//_client.DeleteIndex(new DeleteIndexRequest(IndexName));
			var isExists = _client.IndexExists(new IndexExistsRequest(IndexName)).Exists;
			if (isExists)
				return;
			var createIndexResponse = _client.CreateIndex(IndexName, c => c
				.Settings(s => s
					.Analysis(a => a
						.Analyzers(aa => aa.Stop("pfstop", st => st.StopWords("_english_"))
						)
					)
				)
				.Mappings(m => m
					.Map<SearchTopic>(mm => mm
						.Properties(p => p
							.Text(t => t
								.Name(n => n.Posts)
								.Name(n => n.FirstPost)
								.Name(n => n.Title)
								.SearchAnalyzer("pfstop")
							)
						)
					)
				)
			);
		}
	}
}