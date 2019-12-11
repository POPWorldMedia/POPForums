using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using PopForums.Configuration;
using PopForums.Services;
using Index = Microsoft.Azure.Search.Models.Index;

namespace PopForums.AzureKit.Search
{
	public class SearchIndexSubsystem : ISearchIndexSubsystem
	{
		private readonly ITextParsingService _textParsingService;
		private readonly ISearchService _searchService;
		private readonly IPostService _postService;
		private readonly IConfig _config;
		private readonly ITopicService _topicService;
		private readonly IErrorLog _errorLog;
		public static string IndexName = "popforumstopics";

		public SearchIndexSubsystem(ITextParsingService textParsingService, ISearchService searchService, IPostService postService, IConfig config, ITopicService topicService, IErrorLog errorLog)
		{
			_textParsingService = textParsingService;
			_searchService = searchService;
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
				var serviceClient = new SearchServiceClient(_config.SearchUrl, new SearchCredentials(_config.SearchKey));
				if (!serviceClient.Indexes.Exists(IndexName))
					CreateIndex(serviceClient);

				var posts = _postService.GetPosts(topic, false).Result.ToArray();
				var parsedPosts = posts.Select(x =>
					{
						var parsedText = _textParsingService.ClientHtmlToForumCode(x.FullText);
						parsedText = _textParsingService.RemoveForumCode(parsedText); 
						return parsedText;
					}).ToArray();
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
					Posts = parsedPosts,
					TenantID = tenantID
				};

				var actions =
				new[]
				{
					IndexAction.Upload(searchTopic)
				};
				try
				{
					var serviceIndexClient = serviceClient.Indexes.GetClient(IndexName);
					var batch = IndexBatch.New(actions);
					serviceIndexClient.Documents.Index(batch);
				}
				catch (Exception exc)
				{
					_errorLog.Log(exc, ErrorSeverity.Error);
					_topicService.QueueTopicForIndexing(topic.TopicID);
				}
		    }
	    }

		public void RemoveIndex(int topicID, string tenantID)
		{
			var key = $"{tenantID}-{topicID}";
			try
			{
				var actions = new[]
				{
						IndexAction.Delete("key", key)
				};
				var serviceClient = new SearchServiceClient(_config.SearchUrl, new SearchCredentials(_config.SearchKey));
				var serviceIndexClient = serviceClient.Indexes.GetClient(IndexName);
				var batch = IndexBatch.New(actions);
				serviceIndexClient.Documents.Index(batch);
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
			}
		}

		private static void CreateIndex(SearchServiceClient serviceClient)
	    {
		    var indexDefinition = new Index
		    {
			    Name = IndexName,
			    Fields = new[]
				{
					new Field("key", DataType.String) {IsKey = true, IsSearchable = false},
					new Field("topicID", DataType.String) {IsSearchable = false},
				    new Field("forumID", DataType.Int32) {IsFilterable = true, IsSearchable = false},
				    new Field("title", DataType.String) {IsSearchable = true, IsSortable = true},
					new Field("lastPostTime", DataType.DateTimeOffset) {IsSortable = true, IsSearchable = false},
					new Field("startedByName", DataType.String) {IsSortable = true, IsSearchable = true},
					new Field("replies", DataType.Int32) {IsSortable = true, IsSearchable = false},
					new Field("views", DataType.Int32) {IsSortable = true, IsSearchable = false},
					new Field("isClosed", DataType.Boolean) {IsSortable = false, IsSearchable = false},
					new Field("isPinned", DataType.Boolean) {IsSortable = false, IsSearchable = false},
					new Field("urlName", DataType.String) {IsSortable = false, IsSearchable = false},
					new Field("lastPostName", DataType.String) {IsSortable = false, IsSearchable = false},
					new Field("posts", DataType.Collection(DataType.String)) {IsSortable = false, IsSearchable = true},
					new Field("tenantID", DataType.String) {IsSearchable = true}
			    },
				ScoringProfiles = new []
				{
					new ScoringProfile("TopicWeight", new TextWeights(new Dictionary<string, double>
					{
						{"title", 10},
						{"startedByName", 5},
						{"posts", 1}
					}))
				}
		    };
		    serviceClient.Indexes.Create(indexDefinition);
	    }
    }
}
