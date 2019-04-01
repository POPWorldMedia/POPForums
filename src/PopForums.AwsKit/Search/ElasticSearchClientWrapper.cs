using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.AwsKit.Search
{
	public interface IElasticSearchClientWrapper
	{
		IIndexResponse IndexTopic(SearchTopic searchTopic);
		Response<IEnumerable<int>> SearchTopicsWithIDs(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize, out int topicCount);
		void VerifyIndexCreate();
	}

	public class ElasticSearchClientWrapper : IElasticSearchClientWrapper
	{
		private readonly IErrorLog _errorLog;
		private readonly ITenantService _tenantService;
		private readonly ElasticClient _client;

		private const string IndexName = "topicindex";

		public ElasticSearchClientWrapper(IConfig config, IErrorLog errorLog, ITenantService tenantService)
		{
			_errorLog = errorLog;
			_tenantService = tenantService;
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

		public Response<IEnumerable<int>> SearchTopicsWithIDs(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize, out int topicCount)
		{
			Func<SortDescriptor<SearchTopic>, IPromise<IList<ISort>>> sortSelector;
			switch (searchType)
			{
				case SearchType.Date:
					sortSelector = sort => sort.Descending(d => d.LastPostTime);
					break;
				case SearchType.Name:
					sortSelector = sort => sort.Ascending(d => d.StartedByName).Descending(SortSpecialField.Score);
					break;
				case SearchType.Replies:
					sortSelector = sort => sort.Descending(d => d.Replies);
					break;
				case SearchType.Title:
					sortSelector = sort => sort.Ascending(d => d.Title);
					break;
				default:
					sortSelector = sort => sort.Descending(SortSpecialField.Score);
					break;
			}

			var tenantID = _tenantService.GetTenant();
			startRow--;
			var searchResponse = _client.Search<SearchTopic>(s => s
				.Source(sf => sf.Includes(i => i.Fields(f => f.Id)))
				.Query(q => 
					!q.Terms(set => set.Field(field => field.ForumID).Terms(hiddenForums)) && 
					q.Bool(bb => bb.Must(tt => tt.Term(ff => ff.TenantID, tenantID))) && 
					q.MultiMatch(m => m.Query(searchTerm)
						.Fields(f => f
							.Field(x => x.Title, boost: 5)
							.Field(x => x.FirstPost, boost: 2)
							.Field(x => x.Posts))))
				.Sort(sortSelector)
				.Take(pageSize)
				.Skip(startRow));
			Response<IEnumerable<int>> result;
			if (!searchResponse.IsValid)
			{
				_errorLog.Log(searchResponse.OriginalException, ErrorSeverity.Error, $"Debugging info: {searchResponse.DebugInformation}");
				result = new Response<IEnumerable<int>>(null, false, searchResponse.OriginalException, searchResponse.DebugInformation);
				topicCount = 0;
				return result;
			}
			var ids = searchResponse.Documents.Select(d => Convert.ToInt32(d.Id));
			topicCount = (int)searchResponse.Total;
			result = new Response<IEnumerable<int>>(ids);
			return result;
		}

		public void VerifyIndexCreate()
		{
			var isExists = _client.IndexExists(new IndexExistsRequest(IndexName)).Exists;
			if (isExists)
				return;
			var createIndexResponse = _client.CreateIndex(IndexName, c => c
				.Settings(s => s
					.Analysis(a => a
						.Analyzers(aa => aa
							.Standard("standard_english", sa => sa
								.StopWords("_english_")
							)
						)
					)
				)
				.Mappings(m => m
					.Map<SearchTopic>(mm => mm
						.Properties(p => p
							.Text(t => t
								.Name(n => n.Posts)
								.Analyzer("standard_english")
							)
							.Text(t => t
								.Name(n => n.FirstPost)
								.Analyzer("standard_english")
							)
							.Text(t => t
								.Name(n => n.Title)
								.Analyzer("standard_english")
								.Fielddata(true)
							)
							.Text(t => t
								.Name(n => n.StartedByName)
								.Fielddata(true)
							)
						)
					)
				)
			);
		}
	}
}