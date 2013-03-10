using System;
using System.Collections.Generic;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public class ModerationLogService : IModerationLogService
	{
		public ModerationLogService(IModerationLogRepository moderationLogRepo)
		{
			_moderationLogRepo = moderationLogRepo;
		}

		private readonly IModerationLogRepository _moderationLogRepo;

		public void LogTopic(User user, ModerationType moderationType, Topic topic, Forum forum)
		{
			LogTopic(user, moderationType, topic, forum, String.Empty);
		}

		public void LogTopic(User user, ModerationType moderationType, Topic topic, Forum forum, string comment)
		{
			_moderationLogRepo.Log(DateTime.UtcNow, user.UserID, user.Name, (int) moderationType, forum != null ? forum.ForumID : (int?)null, topic.TopicID, null, comment, String.Empty);
		}

		public void LogPost(User user, ModerationType moderationType, Post post, string comment, string oldText)
		{
			_moderationLogRepo.Log(DateTime.UtcNow, user.UserID, user.Name, (int)moderationType, null, post.TopicID, post.PostID, comment, oldText);
		}

		public List<ModerationLogEntry> GetLog(DateTime start, DateTime end)
		{
			return _moderationLogRepo.GetLog(start, end);
		}

		public List<ModerationLogEntry> GetLog(Topic topic, bool excludePostEntries)
		{
			return _moderationLogRepo.GetLog(topic.TopicID, excludePostEntries);
		}

		public List<ModerationLogEntry> GetLog(Post post)
		{
			return _moderationLogRepo.GetLog(post.PostID);
		}
	}
}