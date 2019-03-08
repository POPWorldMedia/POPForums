using System;
using System.Linq;
using Nest;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.AwsKit.Search
{
	public class SearchIndexSubsystem : ISearchIndexSubsystem
	{
		private readonly ITextParsingService _textParsingService;

		public SearchIndexSubsystem(ITextParsingService textParsingService)
		{
			_textParsingService = textParsingService;
		}

		public void DoIndex(ISearchService searchService, ISettingsManager settingsManager, IPostService postService, IConfig config,
			ITopicService topicService, IErrorLog errorLog)
		{
			var topic = searchService.GetNextTopicForIndexing();
			if (topic == null)
				return;

			var posts = postService.GetPosts(topic, false).ToArray();
			var parsedPosts = posts.Select(x =>
			{
				var parsedText = _textParsingService.ClientHtmlToForumCode(x.FullText);
				parsedText = _textParsingService.RemoveForumCode(parsedText);
				return parsedText;
			}).ToArray();
			var searchTopic = new SearchTopic
			{
				Id = topic.TopicID.ToString(),
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
				Posts = parsedPosts
			};

			try
			{
				var node = new Uri(config.SearchUrl);
				var settings = new ConnectionSettings(node)
					.DefaultIndex(IndexName);
				var client = new ElasticClient(settings);

				var indexResult = client.IndexDocument(searchTopic);
				if (indexResult.Result != Result.Created && indexResult.Result != Result.Updated)
					errorLog.Log(indexResult.OriginalException, ErrorSeverity.Error, $"Debug information: {indexResult.DebugInformation}");
			}
			catch (Exception exc)
			{
				errorLog.Log(exc, ErrorSeverity.Error);
			}
		}

		private const string IndexName = "topicindex";
	}
}