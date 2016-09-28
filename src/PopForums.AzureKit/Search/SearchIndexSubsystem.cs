using System;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.AzureKit.Search
{
	public class SearchIndexSubsystem : ISearchIndexSubsystem
	{
		public static string IndexName = "popforumstopics";

		public void DoIndex(ISearchService searchService, ISettingsManager settingsManager, IPostService postService,
			IConfig config, ITopicService topicService, IErrorLog errorLog)
		{
			var topic = searchService.GetNextTopicForIndexing();
			if (topic != null)
			{
				var serviceClient = new SearchServiceClient(config.SearchUrl, new SearchCredentials(config.SearchKey));
				if (!serviceClient.Indexes.Exists(IndexName))
					CreateIndex(serviceClient);

				var posts = postService.GetPosts(topic, false).ToArray();
				var searchTopic = new SearchTopic
				{
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
					Posts = posts
				};

				var actions = new []
				{
					IndexAction.Upload(searchTopic)
				};
				var batch = IndexBatch.New(actions);
				try
				{
					var serviceIndexClient = new SearchIndexClient(config.SearchUrl, IndexName, new SearchCredentials(config.SearchKey));
					serviceIndexClient.Documents.Index(batch);
					searchService.MarkTopicAsIndexed(topic);
				}
				catch (Exception exc)
				{
					errorLog.Log(exc, ErrorSeverity.Error);
					topicService.MarkTopicForIndexing(topic.TopicID);
				}
		    }
	    }

	    private static void CreateIndex(SearchServiceClient serviceClient)
	    {
		    var indexDefinition = new Index
		    {
			    Name = IndexName,
			    Fields = new[]
			    {
				    new Field("topicID", DataType.String) {IsKey = true, IsSearchable = false},
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
					new Field("posts", DataType.Collection(DataType.String)) {IsSortable = false, IsSearchable = true}
			    }
		    };
		    serviceClient.Indexes.Create(indexDefinition);
	    }
    }
}
