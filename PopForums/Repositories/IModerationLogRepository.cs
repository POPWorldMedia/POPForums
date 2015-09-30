using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IModerationLogRepository
	{
		void Log(DateTime timeStamp, int userID, string userName, int moderationType, int? forumID, int topicID, int? postID, string comment, string oldText);
		List<ModerationLogEntry> GetLog(DateTime start, DateTime end);
		List<ModerationLogEntry> GetLog(int topicID, bool excludePostEntries);
		List<ModerationLogEntry> GetLog(int postID);
	}
}