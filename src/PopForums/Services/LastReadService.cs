using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface ILastReadService
	{
		Task MarkForumRead(User user, Forum forum);
		Task MarkAllForumsRead(User user);
		Task MarkTopicRead(User user, Topic topic);
		Task GetForumReadStatus(User user, CategorizedForumContainer container);
		Task GetTopicReadStatus(User user, PagedTopicContainer container);
		Task<Post> GetFirstUnreadPost(User user, Topic topic);
		Task<DateTime?> GetTopicReadStatus(User user, Topic topic);
		Task<DateTime?> GetForumReadStatus(User user, Forum forum);
		Task<DateTime?> GetLastReadTime(User user, Topic topic);
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

		public async Task MarkForumRead(User user, Forum forum)
		{
			if (user == null)
				throw new ArgumentNullException("user");
			if (forum == null)
				throw new ArgumentNullException("forum");
			await _lastReadRepository.SetForumRead(user.UserID, forum.ForumID, DateTime.UtcNow);
			await _lastReadRepository.DeleteTopicReadsInForum(user.UserID, forum.ForumID);
		}

		public async Task MarkAllForumsRead(User user)
		{
			if (user == null)
				throw new ArgumentNullException("user");
			await _lastReadRepository.SetAllForumsRead(user.UserID, DateTime.UtcNow);
			await _lastReadRepository.DeleteAllTopicReads(user.UserID);
		}

		public async Task MarkTopicRead(User user, Topic topic)
		{
			if (user == null)
				throw new ArgumentNullException("user");
			if (topic == null)
				throw new ArgumentNullException("topic");
			await _lastReadRepository.SetTopicRead(user.UserID, topic.TopicID, DateTime.UtcNow);
		}

		public async Task GetForumReadStatus(User user, CategorizedForumContainer container)
		{
			Dictionary<int, DateTime> lastReads = null;
			if (user != null)
				lastReads = await _lastReadRepository.GetLastReadTimesForForums(user.UserID);
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

		public async Task<DateTime?> GetTopicReadStatus(User user, Topic topic)
		{
			if (user != null)
			{
				return await _lastReadRepository.GetLastReadTimeForTopic(user.UserID, topic.TopicID);
			}
			return null;
		}

		public async Task<DateTime?> GetForumReadStatus(User user, Forum forum)
		{
			if (user != null)
			{
				return await _lastReadRepository.GetLastReadTimesForForum(user.UserID, forum.ForumID);
			}
			return null;
		}

		public async Task GetTopicReadStatus(User user, PagedTopicContainer container)
		{
			Dictionary<int, DateTime> lastForumReads = null;
			Dictionary<int, DateTime> lastTopicReads = null;
			if (user != null)
			{
				lastForumReads = await _lastReadRepository.GetLastReadTimesForForums(user.UserID);
				lastTopicReads = await _lastReadRepository.GetLastReadTimesForTopics(user.UserID, container.Topics.Select(t => t.TopicID));
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

		public async Task<Post> GetFirstUnreadPost(User user, Topic topic)
		{
			if (topic == null)
				throw new ArgumentException("Can't use a null topic.", "topic");
			var includeDeleted = false;
			if (user != null && user.IsInRole(PermanentRoles.Moderator))
				includeDeleted = true;
			var ids = await _postRepository.GetPostIDsWithTimes(topic.TopicID, includeDeleted);
			var postIDs	= ids.Select(d => new { PostID = d.Key, PostTime = d.Value }).ToList();
			if (user == null)
				return await _postRepository.Get(postIDs[0].PostID);
			var lastRead = await _lastReadRepository.GetLastReadTimeForTopic(user.UserID, topic.TopicID);
			if (!lastRead.HasValue)
				lastRead = await _lastReadRepository.GetLastReadTimesForForum(user.UserID, topic.ForumID);
			if (!lastRead.HasValue || !postIDs.Any(p => p.PostTime > lastRead.Value))
				return await _postRepository.Get(postIDs[0].PostID);
			var firstNew = postIDs.First(p => p.PostTime > lastRead.Value);
			return await _postRepository.Get(firstNew.PostID);
		}

		public async Task<DateTime?> GetLastReadTime(User user, Topic topic)
		{
			var lastRead = await _lastReadRepository.GetLastReadTimeForTopic(user.UserID, topic.TopicID);
			if (!lastRead.HasValue)
				lastRead = await _lastReadRepository.GetLastReadTimesForForum(user.UserID, topic.ForumID);
			return lastRead;
		}
	}
}