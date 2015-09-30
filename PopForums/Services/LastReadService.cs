using System;
using System.Collections.Generic;
using System.Linq;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface ILastReadService
	{
		void MarkForumRead(User user, Forum forum);
		void MarkAllForumsRead(User user);
		void MarkTopicRead(User user, Topic topic);
		void GetForumReadStatus(User user, CategorizedForumContainer container);
		void GetTopicReadStatus(User user, PagedTopicContainer container);
		Post GetFirstUnreadPost(User user, Topic topic);
		DateTime? GetTopicReadStatus(User user, Topic topic);
		DateTime? GetForumReadStatus(User user, Forum forum);
		DateTime? GetLastReadTime(User user, Topic topic);
	}

	public class LastReadService : ILastReadService
	{
		public LastReadService(ILastReadRepository lastReadRepository, IPostRepository postRepository)
		{
			_lastReadRepository = lastReadRepository;
			_postRepository = postRepository;
		}

		private readonly ILastReadRepository _lastReadRepository;
		private readonly IPostRepository _postRepository;

		public void MarkForumRead(User user, Forum forum)
		{
			if (user == null)
				throw new ArgumentNullException("user");
			if (forum == null)
				throw new ArgumentNullException("forum");
			_lastReadRepository.SetForumRead(user.UserID, forum.ForumID, DateTime.UtcNow);
			_lastReadRepository.DeleteTopicReadsInForum(user.UserID, forum.ForumID);
		}

		public void MarkAllForumsRead(User user)
		{
			if (user == null)
				throw new ArgumentNullException("user");
			_lastReadRepository.SetAllForumsRead(user.UserID, DateTime.UtcNow);
			_lastReadRepository.DeleteAllTopicReads(user.UserID);
		}

		public void MarkTopicRead(User user, Topic topic)
		{
			if (user == null)
				throw new ArgumentNullException("user");
			if (topic == null)
				throw new ArgumentNullException("topic");
			_lastReadRepository.SetTopicRead(user.UserID, topic.TopicID, DateTime.UtcNow);
		}

		public void GetForumReadStatus(User user, CategorizedForumContainer container)
		{
			Dictionary<int, DateTime> lastReads = null;
			if (user != null)
				lastReads = _lastReadRepository.GetLastReadTimesForForums(user.UserID);
			foreach (var forum in container.AllForums)
			{
				var status = ReadStatus.NoNewPosts;
				if (lastReads != null && lastReads.ContainsKey(forum.ForumID))
					if (forum.LastPostTime > lastReads[forum.ForumID])
						status = ReadStatus.NewPosts;
				if (lastReads != null && !lastReads.ContainsKey(forum.ForumID))
					status = ReadStatus.NewPosts;
				if (forum.IsArchived)
					status |= ReadStatus.Closed;
				container.ReadStatusLookup.Add(forum.ForumID, status);
			}
		}

		public DateTime? GetTopicReadStatus(User user, Topic topic)
		{
			if (user != null)
			{
				return _lastReadRepository.GetLastReadTimeForTopic(user.UserID, topic.TopicID);
			}
			return null;
		}

		public DateTime? GetForumReadStatus(User user, Forum forum)
		{
			if (user != null)
			{
				return _lastReadRepository.GetLastReadTimesForForum(user.UserID, forum.ForumID);
			}
			return null;
		}

		public void GetTopicReadStatus(User user, PagedTopicContainer container)
		{
			Dictionary<int, DateTime> lastForumReads = null;
			Dictionary<int, DateTime> lastTopicReads = null;
			if (user != null)
			{
				lastForumReads = _lastReadRepository.GetLastReadTimesForForums(user.UserID);
				lastTopicReads = _lastReadRepository.GetLastReadTimesForTopics(user.UserID, container.Topics.Select(t => t.TopicID));
			}
			foreach (var topic in container.Topics)
			{
				var status = new ReadStatus();
				if (topic.IsClosed)
					status |= ReadStatus.Closed;
				else
					status |= ReadStatus.Open;
				if (topic.IsPinned)
					status |= ReadStatus.Pinned;
				else
					status |= ReadStatus.NotPinned;
				if (lastForumReads == null)
					status |= ReadStatus.NoNewPosts;
				else
				{
					var lastRead = DateTime.MinValue;
					if (lastForumReads.ContainsKey(topic.ForumID))
						lastRead = lastForumReads[topic.ForumID];
					if (lastTopicReads.ContainsKey(topic.TopicID) && lastTopicReads[topic.TopicID] > lastRead)
						lastRead = lastTopicReads[topic.TopicID];
					if (topic.LastPostTime > lastRead)
						status |= ReadStatus.NewPosts;
					else
						status |= ReadStatus.NoNewPosts;
				}
				container.ReadStatusLookup.Add(topic.TopicID, status);
			}
		}

		public Post GetFirstUnreadPost(User user, Topic topic)
		{
			if (topic == null)
				throw new ArgumentException("Can't use a null topic.", "topic");
			var includeDeleted = false;
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			var postIDs = _postRepository.GetPostIDsWithTimes(topic.TopicID, includeDeleted).Select(d => new { PostID = d.Key, PostTime = d.Value }).ToList();
			if (user == null)
				return _postRepository.Get(postIDs[0].PostID);
			var lastRead = _lastReadRepository.GetLastReadTimeForTopic(user.UserID, topic.TopicID);
			if (!lastRead.HasValue)
				lastRead = _lastReadRepository.GetLastReadTimesForForum(user.UserID, topic.ForumID);
			if (!lastRead.HasValue || !postIDs.Any(p => p.PostTime > lastRead.Value))
				return _postRepository.Get(postIDs[0].PostID);
			var firstNew = postIDs.First(p => p.PostTime > lastRead.Value);
			return _postRepository.Get(firstNew.PostID);
		}

		public DateTime? GetLastReadTime(User user, Topic topic)
		{
			var lastRead = _lastReadRepository.GetLastReadTimeForTopic(user.UserID, topic.TopicID);
			if (!lastRead.HasValue)
				lastRead = _lastReadRepository.GetLastReadTimesForForum(user.UserID, topic.ForumID);
			return lastRead;
		}
	}
}