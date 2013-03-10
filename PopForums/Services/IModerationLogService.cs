using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Services
{
	public interface IModerationLogService
	{
		void LogTopic(User user, ModerationType moderationType, Topic topic, Forum forum);
		void LogTopic(User user, ModerationType moderationType, Topic topic, Forum forum, string comment);
		void LogPost(User user, ModerationType moderationType, Post post, string comment, string oldText);
		List<ModerationLogEntry> GetLog(DateTime start, DateTime end);
		List<ModerationLogEntry> GetLog(Topic topic, bool excludePostEntries);
		List<ModerationLogEntry> GetLog(Post post);
	}
}