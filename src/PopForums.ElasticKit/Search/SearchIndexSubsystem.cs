using System;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Polly;
using Polly.Retry;
using PopForums.Configuration;
using PopForums.Services;

namespace PopForums.ElasticKit.Search;

public class SearchIndexSubsystem : ISearchIndexSubsystem
{
	private readonly ITextParsingService _textParsingService;
	private readonly IPostService _postService;
	private readonly ITopicService _topicService;
	private readonly IErrorLog _errorLog;
	private readonly IElasticSearchClientWrapper _elasticSearchClientWrapper;

	public SearchIndexSubsystem(ITextParsingService textParsingService, IPostService postService, ITopicService topicService, IErrorLog errorLog, IElasticSearchClientWrapper elasticSearchClientWrapper)
	{
		_textParsingService = textParsingService;
		_postService = postService;
		_topicService = topicService;
		_errorLog = errorLog;
		_elasticSearchClientWrapper = elasticSearchClientWrapper;
	}

	public void DoIndex(int topicID, string tenantID, bool isForRemoval)
	{
		if (isForRemoval)
		{
			RemoveIndex(topicID, tenantID);
			return;
		}

		var topic = _topicService.Get(topicID).Result;
		if (topic == null)
			return;

		_elasticSearchClientWrapper.VerifyIndexCreate();

		var posts = _postService.GetPosts(topic, false).Result;
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
			Id = $"{tenantID}-{topic.TopicID}",
			TopicID = topic.TopicID,
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
			var pipeline = new ResiliencePipelineBuilder<IndexResponse>()
				.AddRetry(new RetryStrategyOptions<IndexResponse>
				{
					ShouldHandle = new PredicateBuilder<IndexResponse>()
						.HandleResult(static result => !result.IsValidResponse),
					DelayGenerator = static args =>
					{
						var delay = args.AttemptNumber switch
						{
							0 => TimeSpan.FromSeconds(1),
							1 => TimeSpan.FromSeconds(5),
							_ => TimeSpan.FromSeconds(30)
						};
						return new ValueTask<TimeSpan?>(delay);
					},
					OnRetry = responseArgs =>
					{
						_errorLog.Log(responseArgs.Outcome.Exception, ErrorSeverity.Error,
							$"Retry after {responseArgs.Duration.Seconds}: {responseArgs.Outcome.Result?.DebugInformation}");
						return default;
					}
				}).Build();
			
			pipeline.Execute(() =>
			{
				var indexResult = _elasticSearchClientWrapper.IndexTopic(searchTopic);
				return indexResult;
			});
		}
		catch (Exception exc)
		{
			_errorLog.Log(exc, ErrorSeverity.Error);
		}
	}

	public void RemoveIndex(int topicID, string tenantID)
	{
		var id = $"{tenantID}-{topicID}";

		try
		{
			var result = _elasticSearchClientWrapper.RemoveTopic(id);
			if (result.Result != Result.Deleted)
			{
				result.TryGetOriginalException(out var exc);
				_errorLog.Log(exc, ErrorSeverity.Error, $"Debug information: {result.DebugInformation}");
			}
		}
		catch (Exception exc)
		{
			_errorLog.Log(exc, ErrorSeverity.Error);
		}
	}
}