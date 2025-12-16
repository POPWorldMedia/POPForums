using System;
using System.Collections.Generic;
using System.Linq;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Services;
using SearchType = PopForums.Models.SearchType;

namespace PopForums.ElasticKit.Search;

public interface IElasticSearchClientWrapper
{
	IndexResponse IndexTopic(SearchTopic searchTopic);
	Response<IEnumerable<int>> SearchTopicsWithIDs(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize, out int topicCount);
	void VerifyIndexCreate();
	DeleteResponse RemoveTopic(string id);
}

public class ElasticSearchClientWrapper : IElasticSearchClientWrapper
{
	private readonly IErrorLog _errorLog;
	private readonly ITenantService _tenantService;
	private readonly ElasticsearchClient _client;

	private const string IndexName = "topicindex";

	public ElasticSearchClientWrapper(IConfig config, IErrorLog errorLog, ITenantService tenantService)
	{
		_errorLog = errorLog;
		_tenantService = tenantService;
		ElasticsearchClientSettings settings;
		switch (config.SearchProvider.ToLower())
		{
			case "elasticsearch":
				settings = new ElasticsearchClientSettings(new Uri(config.SearchUrl))
					.DefaultIndex(IndexName).DisableDirectStreaming()
					.Authentication(new ApiKey(config.SearchKey));
				break;
			default:
				settings = new ElasticsearchClientSettings()
					.DefaultIndex(IndexName).DisableDirectStreaming();
				break;
		}
		
		_client = new ElasticsearchClient(settings);
	}

	public IndexResponse IndexTopic(SearchTopic searchTopic)
	{
		var tenantID = _tenantService.GetTenant();
		if (string.IsNullOrWhiteSpace(tenantID))
			tenantID = "-";
		searchTopic.TenantID = tenantID;
		var indexResult = _client.Index(searchTopic);
		return indexResult;
	}

	public DeleteResponse RemoveTopic(string id)
	{
		var deleteRequest = new DeleteRequest(IndexName, id);
		var response = _client.Delete(deleteRequest);
		return response;
	}

	public Response<IEnumerable<int>> SearchTopicsWithIDs(string searchTerm, List<int> hiddenForums, SearchType searchType, int startRow, int pageSize, out int topicCount)
	{
		var sortSelector = new SortOptionsDescriptor<SearchTopic>();
		switch (searchType)
		{
			case SearchType.Date:
				sortSelector.Field(sort => sort.LastPostTime, config => config.Order(SortOrder.Desc));
				break;
			case SearchType.Name:
				sortSelector.Field(sort => sort.StartedByName, config => config.Order(SortOrder.Asc));
				break;
			case SearchType.Replies:
				sortSelector.Field(sort => sort.Replies, config => config.Order(SortOrder.Desc));
				break;
			case SearchType.Title:
				sortSelector.Field(sort => sort.Title, config => config.Order(SortOrder.Asc));
				break;
			default:
				sortSelector.Score(config => config.Order(SortOrder.Desc));
				break;
		}

		var tenantID = _tenantService.GetTenant();
		if (string.IsNullOrWhiteSpace(tenantID))
			tenantID = "-";
		startRow--;
		var searchResponse = _client.Search<SearchTopic>(s => s
			.Query(q => q
				.Bool(bb => bb
					.Must(ff => ff
						.MultiMatch(m => m
							.Query(searchTerm)
							.Fields(new [] { "title^10", "firstPost^5", "posts" })
							.Fuzziness(new Fuzziness("auto")))
					)
					.MustNot(ff => ff
						.Terms(m => m
							.Field(f => f.ForumID)
								.Terms(new TermsQueryField(hiddenForums.Select(s => (FieldValue)s).ToArray())))
					)
					 .Filter(ff => ff
					 	.Term(t => t
					 		.Field(f => f.TenantID).Value(tenantID))
					)
				)
			)
			.SourceIncludes(new []{"topicID"})
			.Sort(sortSelector)
			.From(startRow)
			.Size(pageSize));
		Response<IEnumerable<int>> result;
		if (!searchResponse.IsValidResponse)
		{
			searchResponse.TryGetOriginalException(out var exception);
			_errorLog.Log(exception, ErrorSeverity.Error, $"Debugging info: {searchResponse.DebugInformation}");
			result = new Response<IEnumerable<int>>(null, false, exception, searchResponse.DebugInformation);
			topicCount = 0;
			return result;
		}
		var ids = searchResponse.Documents.Select(d => d.TopicID);
		topicCount = (int)searchResponse.Total;
		result = new Response<IEnumerable<int>>(ids);
		return result;
	}

	public void VerifyIndexCreate()
	{
		var isExists = _client.Indices.Exists(Indices.Index(IndexName)).Exists;
		if (isExists)
			return;
		var createIndexResponse = _client.Indices.Create(IndexName, c => c
			.Settings(s => s
				.Analysis(a => a
					.Analyzers(aa => aa
						.Standard("standard_english", sa => sa
							.Stopwords(new List<string> { "_english_" })
						)
					)
				)
			)
			.Mappings(mm => mm
				.Properties<SearchTopic>(p => p
					.Text(t => t.Posts)
					.Text(t => t.FirstPost)
					.Text(t => t.Title, tp => tp.Fielddata())
					.Text(t => t.StartedByName, tp => tp.Fielddata())
					.Keyword(t => t.TenantID))
			)
		);
		if (!createIndexResponse.IsValidResponse)
		{
			createIndexResponse.TryGetOriginalException(out var exception);
			_errorLog.Log(exception, ErrorSeverity.Error, createIndexResponse.DebugInformation);
		}
	}
}