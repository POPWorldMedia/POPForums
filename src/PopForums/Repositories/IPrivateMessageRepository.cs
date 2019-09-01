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
		Task<List<PrivateMessageUser>> GetUsers(int pmID);
		Task SetLastViewTime(int pmID, int userID, DateTime viewDate);
		Task SetArchive(int pmID, int userID, bool isArchived);
		Task<List<PrivateMessage>> GetPrivateMessages(int userID, PrivateMessageBoxType boxType, int startRow, int pageSize);
		Task<int> GetUnreadCount(int userID);
		Task<int> GetBoxCount(int userID, PrivateMessageBoxType boxType);
		Task UpdateLastPostTime(int pmID, DateTime lastPostTime);
	}
}
