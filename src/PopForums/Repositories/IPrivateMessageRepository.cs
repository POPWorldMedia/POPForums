using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IPrivateMessageRepository
	{
		Task<PrivateMessage> Get(int pmID);
		Task<List<PrivateMessagePost>> GetPosts(int pmID);
		Task<int> CreatePrivateMessage(PrivateMessage pm);
		Task AddUsers(int pmID, List<int> userIDs, DateTime viewDate, bool isArchived);
		Task<int> AddPost(PrivateMessagePost post);
		List<PrivateMessageUser> GetUsers(int pmID);
		void SetLastViewTime(int pmID, int userID, DateTime viewDate);
		void SetArchive(int pmID, int userID, bool isArchived);
		List<PrivateMessage> GetPrivateMessages(int userID, PrivateMessageBoxType boxType, int startRow, int pageSize);
		int GetUnreadCount(int userID);
		int GetBoxCount(int userID, PrivateMessageBoxType boxType);
		void UpdateLastPostTime(int pmID, DateTime lastPostTime);
	}
}
