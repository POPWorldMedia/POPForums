using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IModerationLogRepository
	{
		Task Log(DateTime timeStamp, int userID, string userName, int moderationType, int? forumID, int topicID, int? postID, string comment, string oldText);
		Task<List<ModerationLogEntry>> GetLog(DateTime start, DateTime end);
		Task<List<ModerationLogEntry>> GetLog(int topicID, bool excludePostEntries);
		Task<List<ModerationLogEntry>> GetLog(int postID);
	}
}