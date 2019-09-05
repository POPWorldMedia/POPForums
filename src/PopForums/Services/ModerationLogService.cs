using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IModerationLogService
	{
		Task LogTopic(User user, ModerationType moderationType, Topic topic, Forum forum);
		Task LogTopic(User user, ModerationType moderationType, Topic topic, Forum forum, string comment);
		Task LogTopic(ModerationType moderationType, int topicID);
		Task LogPost(User user, ModerationType moderationType, Post post, string comment, string oldText);
		Task<List<ModerationLogEntry>> GetLog(DateTime start, DateTime end);
		Task<List<ModerationLogEntry>> GetLog(Topic topic, bool excludePostEntries);
		Task<List<ModerationLogEntry>> GetLog(Post post);
	}

	public class ModerationLogService : IModerationLogService
	{
		public ModerationLogService(IModerationLogRepository moderationLogRepo)
		{
			_moderationLogRepo = moderationLogRepo;
		}

		private readonly IModerationLogRepository _moderationLogRepo;

		public async Task LogTopic(User user, ModerationType moderationType, Topic topic, Forum forum)
		{
			await LogTopic(user, moderationType, topic, forum, String.Empty);
		}

		public async Task LogTopic(User user, ModerationType moderationType, Topic topic, Forum forum, string comment)
		{
			await _moderationLogRepo.Log(DateTime.UtcNow, user.UserID, user.Name, (int) moderationType, forum != null ? forum.ForumID : (int?)null, topic.TopicID, null, comment, string.Empty);
		}

		public async Task LogTopic(ModerationType moderationType, int topicID)
		{
			await _moderationLogRepo.Log(DateTime.UtcNow, 0, "System", (int)moderationType, null, topicID, null, string.Empty, string.Empty);
		}

		public async Task LogPost(User user, ModerationType moderationType, Post post, string comment, string oldText)
		{
			await _moderationLogRepo.Log(DateTime.UtcNow, user.UserID, user.Name, (int)moderationType, null, post.TopicID, post.PostID, comment, oldText);
		}

		public async Task<List<ModerationLogEntry>> GetLog(DateTime start, DateTime end)
		{
			return await _moderationLogRepo.GetLog(start, end);
		}

		public async Task<List<ModerationLogEntry>> GetLog(Topic topic, bool excludePostEntries)
		{
			return await _moderationLogRepo.GetLog(topic.TopicID, excludePostEntries);
		}

		public async Task<List<ModerationLogEntry>> GetLog(Post post)
		{
			return await _moderationLogRepo.GetLog(post.PostID);
		}
	}
}