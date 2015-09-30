using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IPrivateMessageRepository
	{
		PrivateMessage Get(int pmID);
		List<PrivateMessagePost> GetPosts(int pmID);
		int CreatePrivateMessage(PrivateMessage pm);
		void AddUsers(int pmID, List<int> userIDs, DateTime viewDate, bool isArchived);
		int AddPost(PrivateMessagePost post);
		List<PrivateMessageUser> GetUsers(int pmID);
		void SetLastViewTime(int pmID, int userID, DateTime viewDate);
		void SetArchive(int pmID, int userID, bool isArchived);
		List<PrivateMessage> GetPrivateMessages(int userID, PrivateMessageBoxType boxType, int startRow, int pageSize);
		int GetUnreadCount(int userID);
		int GetBoxCount(int userID, PrivateMessageBoxType boxType);
		void UpdateLastPostTime(int pmID, DateTime lastPostTime);
	}
}
