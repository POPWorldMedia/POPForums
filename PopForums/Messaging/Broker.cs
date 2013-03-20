using System;
using Microsoft.AspNet.SignalR;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using Topic = PopForums.Models.Topic;

namespace PopForums.Messaging
{
	public interface IBroker
	{
		void NotifyNewPosts(Topic topic, int lasPostID);
		void NotifyFeed(string message);
		void NotifyForumUpdate(Forum forum);
		void NotifyTopicUpdate(Topic topic, Forum forum, string topicLink);
		void NotifyNewPost(Topic topic, int postID);
	}

	public class Broker : IBroker
	{
		public Broker(ITimeFormattingService timeFormattingService, IForumRepository forumRepo)
		{
			_timeFormattingService = timeFormattingService;
			_forumRepo = forumRepo;
		}

		private readonly ITimeFormattingService _timeFormattingService;
		private readonly IForumRepository _forumRepo;

		public void NotifyNewPosts(Topic topic, int lasPostID)
		{
			var context = GlobalHost.ConnectionManager.GetHubContext<Topics>();
			context.Clients.Group(topic.TopicID.ToString()).notifyNewPosts(lasPostID);
		}

		public void NotifyNewPost(Topic topic, int postID)
		{
			var context = GlobalHost.ConnectionManager.GetHubContext<Topics>();
			context.Clients.Group(topic.TopicID.ToString()).fetchNewPost(postID);
		}

		public void NotifyFeed(string message)
		{
			var context = GlobalHost.ConnectionManager.GetHubContext<Feed>();
			var data = new {Message = message, Utc = new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Unspecified).ToString("o"), TimeStamp = Resources.LessThanMinute};
			context.Clients.All.notifyFeed(data);
		}

		public void NotifyForumUpdate(Forum forum)
		{
			_timeFormattingService.Init(null);
			var context = GlobalHost.ConnectionManager.GetHubContext<Forums>();
			context.Clients.All.notifyForumUpdate(new { forum.ForumID, TopicCount = forum.TopicCount.ToString("N0"), PostCount = forum.PostCount.ToString("N0"), LastPostTime = _timeFormattingService.GetFormattedTime(forum.LastPostTime), forum.LastPostName, Utc = forum.LastPostTime.ToString("o"), Image = "NewIndicator.png" });
		}

		public void NotifyTopicUpdate(Topic topic, Forum forum, string topicLink)
		{
			var isForumViewRestricted = _forumRepo.GetForumViewRoles(forum.ForumID).Count > 0;
			_timeFormattingService.Init(null);
			var recentHubContext = GlobalHost.ConnectionManager.GetHubContext<Recent>();
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
					LastPostTime = _timeFormattingService.GetFormattedTime(topic.LastPostTime),
					Utc = topic.LastPostTime.ToString("o"),
					topic.LastPostName
				};
			var forumHubContext = GlobalHost.ConnectionManager.GetHubContext<Forums>();
			if (isForumViewRestricted)
				recentHubContext.Clients.Group("forum" + forum.ForumID).notifyRecentUpdate(result);
			else
				recentHubContext.Clients.All.notifyRecentUpdate(result);
			forumHubContext.Clients.Group(forum.ForumID.ToString()).notifyUpdatedTopic(result);
		}
	}
}