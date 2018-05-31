using System;
using Microsoft.AspNetCore.SignalR;
using PopForums.Messaging;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Messaging
{
    public class Broker : IBroker
	{
		// TODO: test all of these!
		public Broker(ITimeFormattingService timeFormattingService, IForumRepository forumRepo, IHubContext<TopicsHub> topicHubContext, IHubContext<FeedHub> feedHubContext, IHubContext<ForumsHub> forumsHubContext, IHubContext<RecentHub> recentHubContext)
		{
			_timeFormattingService = timeFormattingService;
			_forumRepo = forumRepo;
			_topicHubContext = topicHubContext;
			_feedHubContext = feedHubContext;
			_forumsHubContext = forumsHubContext;
			_recentHubContext = recentHubContext;
		}

		private readonly ITimeFormattingService _timeFormattingService;
		private readonly IForumRepository _forumRepo;
		private readonly IHubContext<TopicsHub> _topicHubContext;
		private readonly IHubContext<FeedHub> _feedHubContext;
		private readonly IHubContext<ForumsHub> _forumsHubContext;
		private readonly IHubContext<RecentHub> _recentHubContext;

		public void NotifyNewPosts(Topic topic, int lasPostID)
		{
			_topicHubContext.Clients.Group(topic.TopicID.ToString()).SendAsync("notifyNewPosts", lasPostID);
		}

		public void NotifyNewPost(Topic topic, int postID)
		{
			_topicHubContext.Clients.Group(topic.TopicID.ToString()).SendAsync("fetchNewPost", postID);
		}

		public void NotifyFeed(string message)
		{
			var data = new { Message = message, Utc = new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Unspecified).ToString("o"), TimeStamp = Resources.LessThanMinute };
			_feedHubContext.Clients.All.SendAsync("notifyFeed", data);
		}

		public void NotifyForumUpdate(Forum forum)
		{
			_forumsHubContext.Clients.All.SendAsync("notifyForumUpdate", new { forum.ForumID, TopicCount = forum.TopicCount.ToString("N0"), PostCount = forum.PostCount.ToString("N0"), LastPostTime = _timeFormattingService.GetFormattedTime(forum.LastPostTime, null), forum.LastPostName, Utc = forum.LastPostTime.ToString("o"), Image = "NewIndicator.png" });
		}

		public void NotifyTopicUpdate(Topic topic, Forum forum, string topicLink)
		{
			var isForumViewRestricted = _forumRepo.GetForumViewRoles(forum.ForumID).Count > 0;
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
			if (isForumViewRestricted)
				_recentHubContext.Clients.Group("forum" + forum.ForumID).SendAsync("notifyRecentUpdate", result);
			else
				_recentHubContext.Clients.All.SendAsync("notifyRecentUpdate", result);
			_forumsHubContext.Clients.Group(forum.ForumID.ToString()).SendAsync("notifyUpdatedTopic", result);
		}
	}
}