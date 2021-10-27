using System;
using System.Collections.Generic;
using System.Linq;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.AzureKit.Search;

public class SearchIndexSubsystem : ISearchIndexSubsystem
{
	private readonly ITextParsingService _textParsingService;
	private readonly IPostService _postService;
	private readonly IConfig _config;
	private readonly ITopicService _topicService;
	private readonly IErrorLog _errorLog;
	public static string IndexName = "popforumstopics";

	public SearchIndexSubsystem(ITextParsingService textParsingService, IPostService postService, IConfig config, ITopicService topicService, IErrorLog errorLog)
	{
		_textParsingService = textParsingService;
		_postService = postService;
		_config = config;
		_topicService = topicService;
		_errorLog = errorLog;
	}

	public void DoIndex(int topicID, string tenantID, bool isForRemoval)
	{
		if (isForRemoval)
		{
			RemoveIndex(topicID, tenantID);
			return;
		}

		var topic = _topicService.Get(topicID).Result;
		if (topic != null)
		{
			var posts = _postService.GetPosts(topic, false).Result.ToArray();
			var parsedPosts = posts.Select(x =>
			{
				var parsedText = _textParsingService.ClientHtmlToForumCode(x.FullText);
				parsedText = _textParsingService.RemoveForumCode(parsedText); 
				return parsedText;
			}).ToArray();
			var joinedPosts = string.Join(" ", parsedPosts);
			var searchTopic = new SearchTopic
			{
				Key = $"{tenantID}-{topicID}",
				TopicID = topic.TopicID.ToString(),
				ForumID = topic.ForumID,
				Title = topic.Title,
				LastPostTime = topic.LastPostTime,
				StartedByName = topic.StartedByName,
				Replies = topic.ReplyCount,
				Views = topic.ViewCount,
				IsClosed = topic.IsClosed,
				IsPinned = topic.IsPinned,
				UrlName = topic.UrlName,
				LastPostName = topic.LastPostName,
				Posts = joinedPosts,
				TenantID = tenantID
			};

			var batch = IndexDocumentsBatch.Create(IndexDocumentsAction.Upload(searchTopic));

			try
			{
				CreateIndex();
				var searchClient = new SearchClient(new Uri(_config.SearchUrl), IndexName, new AzureKeyCredential(_config.SearchKey));
				searchClient.IndexDocuments(batch);
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
			}
		}
	}

	public void RemoveIndex(int topicID, string tenantID)
	{
		var key = $"{tenantID}-{topicID}";
		try
		{
			var searchClient = new SearchClient(new Uri(_config.SearchUrl), IndexName, new AzureKeyCredential(_config.SearchKey));
			searchClient.DeleteDocuments("key", new[] {key});
		}
		catch (Exception exc)
		{
			_errorLog.Log(exc, ErrorSeverity.Error);
		}
	}

	private void CreateIndex()
	{
		var searchIndexClient = new SearchIndexClient(new Uri(_config.SearchUrl), new AzureKeyCredential(_config.SearchKey));
		var indexResult = searchIndexClient.GetIndexNames();
		if (indexResult.Contains(IndexName))
			return;
			
		var indexDefinition = new SearchIndex(IndexName)
		{
			Fields =
			{
				new SimpleField("Key", SearchFieldDataType.String) {IsKey = true},
				new SimpleField("TopicID", SearchFieldDataType.String),
				new SimpleField("ForumID", SearchFieldDataType.Int32) {IsFilterable = true},
				new SearchableField("Title") {IsSortable = true},
				new SimpleField("LastPostTime", SearchFieldDataType.DateTimeOffset) {IsSortable = true},
				new SearchableField("StartedByName") {IsSortable = true},
				new SimpleField("Replies", SearchFieldDataType.Int32) {IsSortable = true},
				new SimpleField("Views", SearchFieldDataType.Int32) {IsSortable = true},
				new SimpleField("IsClosed", SearchFieldDataType.Boolean) {IsSortable = false},
				new SimpleField("IsPinned", SearchFieldDataType.Boolean) {IsSortable = false},
				new SimpleField("UrlName", SearchFieldDataType.String) {IsSortable = false},
				new SimpleField("LastPostName", SearchFieldDataType.String) {IsSortable = false},
				new SearchableField("Posts") {IsSortable = false},
				new SearchableField("TenantID")
			}
		};
		var weights = new TextWeights(new Dictionary<string, double> {{"Title", 10}, {"StartedByName", 5}, {"Posts", 1}});
		indexDefinition.ScoringProfiles.Add(
			new ScoringProfile("TopicWeight") {TextWeights = weights});
		searchIndexClient.CreateIndex(indexDefinition);
	}
}