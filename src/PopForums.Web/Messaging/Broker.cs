using System;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Web.Messaging
{
    public class Broker : IBroker
	{
		// TODO: test all of these!
		public Broker(ITimeFormattingService timeFormattingService, IForumRepository forumRepo, IConnectionManager connectionManager)
		{
			_timeFormattingService = timeFormattingService;
			_forumRepo = forumRepo;
			_connectionManager = connectionManager;
		}

		private readonly ITimeFormattingService _timeFormattingService;
		private readonly IForumRepository _forumRepo;
	    private readonly IConnectionManager _connectionManager;

	    public void NotifyNewPosts(Topic topic, int lasPostID)
		{
			var context = _connectionManager.GetHubContext<TopicsHub>();
			context.Clients.Group(topic.TopicID.ToString()).notifyNewPosts(lasPostID);
		}

		public void NotifyNewPost(Topic topic, int postID)
		{
			var context = _connectionManager.GetHubContext<TopicsHub>();
			context.Clients.Group(topic.TopicID.ToString()).fetchNewPost(postID);
		}

		public void NotifyFeed(string message)
		{
			var context = _connectionManager.GetHubContext<FeedHub>();
			var data = new { Message = message, Utc = new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Unspecified).ToString("o"), TimeStamp = Resources.LessThanMinute };
			context.Clients.All.notifyFeed(data);
		}

		public void NotifyForumUpdate(Forum forum)
		{
			var context = _connectionManager.GetHubContext<ForumsHub>();
			context.Clients.All.notifyForumUpdate(new { forum.ForumID, TopicCount = forum.TopicCount.ToString("N0"), PostCount = forum.PostCount.ToString("N0"), LastPostTime = _timeFormattingService.GetFormattedTime(forum.LastPostTime, null), forum.LastPostName, Utc = forum.LastPostTime.ToString("o"), Image = "NewIndicator.png" });
		}

		public void NotifyTopicUpdate(Topic topic, Forum forum, string topicLink)
		{
			var isForumViewRestricted = _forumRepo.GetForumViewRoles(forum.ForumID).Count > 0;
			var recentHubContext = _connectionManager.GetHubContext<RecentHub>();
			var result = new
			{
				Link = topicLink,
				Image = "NewIndicator.png",
				topic.TopicID,
				topic.StartedByName,
				topic.Title,
				ForumTitle = forum.Title,
				topic.ViewCount,
				topic.ReplyCount,
				LastPostTime = _timeFormattingService.GetFormattedTime(topic.LastPostTime, null),
				Utc = topic.LastPostTime.ToString("o"),
				topic.LastPostName
			};
			var forumHubContext = _connectionManager.GetHubContext<ForumsHub>();
			if (isForumViewRestricted)
				recentHubContext.Clients.Group("forum" + forum.ForumID).notifyRecentUpdate(result);
			else
				recentHubContext.Clients.All.notifyRecentUpdate(result);
			forumHubContext.Clients.Group(forum.ForumID.ToString()).notifyUpdatedTopic(result);
		}
	}
}