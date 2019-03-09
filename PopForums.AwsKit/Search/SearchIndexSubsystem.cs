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
		private readonly IPostService _postService;
		private readonly ITopicService _topicService;
		private readonly IErrorLog _errorLog;
		private readonly ITenantService _tenantService;
		private readonly IElasticSearchClientWrapper _elasticSearchClientWrapper;

		public SearchIndexSubsystem(ITextParsingService textParsingService, ISearchService searchService, IPostService postService, ITopicService topicService, IErrorLog errorLog, ITenantService tenantService, IElasticSearchClientWrapper elasticSearchClientWrapper)
		{
			_textParsingService = textParsingService;
			_searchService = searchService;
			_postService = postService;
			_topicService = topicService;
			_errorLog = errorLog;
			_tenantService = tenantService;
			_elasticSearchClientWrapper = elasticSearchClientWrapper;
		}

		public void DoIndex()
		{
			var topic = _searchService.GetNextTopicForIndexing();
			if (topic == null)
				return;

			var posts = _postService.GetPosts(topic, false);
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
				var indexResult = _elasticSearchClientWrapper.IndexTopic(searchTopic);
				if (indexResult.Result != Result.Created && indexResult.Result != Result.Updated)
				{
					_errorLog.Log(indexResult.OriginalException, ErrorSeverity.Error, $"Debug information: {indexResult.DebugInformation}");
					// TODO: Replace this with some Polly or get real about queues/deadletter
					_topicService.MarkTopicForIndexing(topic.TopicID);
				}
				else
				{
					_searchService.MarkTopicAsIndexed(topic);
				}
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
				// TODO: Replace this with some Polly or get real about queues/deadletter
				_topicService.MarkTopicForIndexing(topic.TopicID);
			}
		}
	}
}