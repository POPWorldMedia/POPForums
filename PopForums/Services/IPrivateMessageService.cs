using System.Collections.Generic;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IPrivateMessageService
	{
		IPrivateMessageRepository PrivateMessageRepository { get; }
		PrivateMessage Get(int pmID);
		List<PrivateMessagePost> GetPosts(PrivateMessage pm);
		List<PrivateMessage> GetPrivateMessages(User user, PrivateMessageBoxType boxType, int pageIndex, out PagerContext pagerContext);
		int GetUnreadCount(User user);
		PrivateMessage Create(string subject, string fullText, User user, List<User> toUsers);
		void Reply(PrivateMessage pm, string fullText, User user);
		bool IsUserInPM(User user, PrivateMessage pm);
		void MarkPMRead(User user, PrivateMessage pm);
		void Archive(User user, PrivateMessage pm);
		void Unarchive(User user, PrivateMessage pm);
	}
}