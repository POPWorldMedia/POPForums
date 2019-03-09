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
		private readonly ISearchService _searchService;
		private readonly ISettingsManager _settingsManager;
		private readonly IPostService _postService;
		private readonly IConfig _config;
		private readonly ITopicService _topicService;
		private readonly IErrorLog _errorLog;
		private readonly ITenantService _tenantService;

		public SearchIndexSubsystem(ITextParsingService textParsingService, ISearchService searchService, ISettingsManager settingsManager, IPostService postService, IConfig config,
			ITopicService topicService, IErrorLog errorLog, ITenantService tenantService)
		{
			_textParsingService = textParsingService;
			_searchService = searchService;
			_settingsManager = settingsManager;
			_postService = postService;
			_config = config;
			_topicService = topicService;
			_errorLog = errorLog;
			_tenantService = tenantService;
		}

		public void DoIndex()
		{
			var topic = _searchService.GetNextTopicForIndexing();
			if (topic == null)
				return;

			var posts = _postService.GetPosts(topic, false).ToArray();
			var parsedPosts = posts.Select(x =>
			{
				var parsedText = _textParsingService.ClientHtmlToForumCode(x.FullText);
				parsedText = _textParsingService.RemoveForumCode(parsedText);
				return parsedText;
			}).ToArray();
			var tenantID = _tenantService.GetTenant();
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
				Posts = parsedPosts,
				TenantID = tenantID
			};

			try
			{
				var node = new Uri(_config.SearchUrl);
				var settings = new ConnectionSettings(node)
					.DefaultIndex(IndexName);
				var client = new ElasticClient(settings);

				var indexResult = client.IndexDocument(searchTopic);
				if (indexResult.Result != Result.Created && indexResult.Result != Result.Updated)
					_errorLog.Log(indexResult.OriginalException, ErrorSeverity.Error, $"Debug information: {indexResult.DebugInformation}");
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
			}
		}

		private const string IndexName = "topicindex";
	}
}