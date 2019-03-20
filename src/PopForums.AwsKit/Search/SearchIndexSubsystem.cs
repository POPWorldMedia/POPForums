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
		private readonly IElasticSearchClientWrapper _elasticSearchClientWrapper;

		public SearchIndexSubsystem(ITextParsingService textParsingService, ISearchService searchService, IPostService postService, ITopicService topicService, IErrorLog errorLog, IElasticSearchClientWrapper elasticSearchClientWrapper)
		{
			_textParsingService = textParsingService;
			_searchService = searchService;
			_postService = postService;
			_topicService = topicService;
			_errorLog = errorLog;
			_elasticSearchClientWrapper = elasticSearchClientWrapper;
		}

		public void DoIndex(int topicID, string tenantID)
		{
			var topic = _topicService.Get(topicID);
			if (topic == null)
				return;

			_elasticSearchClientWrapper.VerifyIndexCreate();

			var posts = _postService.GetPosts(topic, false);
			if (posts.Count == 0)
				throw new Exception($"TopicID {topic.TopicID} has no posts to index.");
			var firstPost = _textParsingService.ClientHtmlToForumCode(posts[0].FullText);
			firstPost = _textParsingService.RemoveForumCode(firstPost);
			posts.RemoveAt(0);
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
				FirstPost = firstPost,
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
					_topicService.QueueTopicForIndexing(topic.TopicID);
				}
			}
			catch (Exception exc)
			{
				_errorLog.Log(exc, ErrorSeverity.Error);
				// TODO: Replace this with some Polly or get real about queues/deadletter
				_topicService.QueueTopicForIndexing(topic.TopicID);
			}
		}
	}
}