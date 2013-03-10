using System;
using System.Collections.Generic;
using System.Linq;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public class PrivateMessageService : IPrivateMessageService
	{
		public PrivateMessageService(IPrivateMessageRepository privateMessageRepo, ISettingsManager settingsManager, ITextParsingService textParsingService)
		{
			PrivateMessageRepository = privateMessageRepo;
			SettingsManager = settingsManager;
			TextParsingService = textParsingService;
		}

		public IPrivateMessageRepository PrivateMessageRepository { get; private set; }
		public ISettingsManager SettingsManager { get; private set; }
		public ITextParsingService TextParsingService { get; private set; }

		public PrivateMessage Get(int pmID)
		{
			return PrivateMessageRepository.Get(pmID);
		}

		public List<PrivateMessagePost> GetPosts(PrivateMessage pm)
		{
			return PrivateMessageRepository.GetPosts(pm.PMID);
		}

		public List<PrivateMessage> GetPrivateMessages(User user, PrivateMessageBoxType boxType, int pageIndex, out PagerContext pagerContext)
		{
			var total = PrivateMessageRepository.GetBoxCount(user.UserID, boxType);
			var pageSize = SettingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(total) / Convert.ToDouble(pageSize)));
			pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			return PrivateMessageRepository.GetPrivateMessages(user.UserID, boxType, startRow, pageSize);
		}

		public int GetUnreadCount(User user)
		{
			return PrivateMessageRepository.GetUnreadCount(user.UserID);
		}

		public PrivateMessage Create(string subject, string fullText, User user, List<User> toUsers)
		{
			if (String.IsNullOrWhiteSpace(subject))
				throw new ArgumentNullException("subject");
			if (String.IsNullOrWhiteSpace(fullText))
				throw new ArgumentNullException("fullText");
			if (user == null)
				throw new ArgumentNullException("user");
			if (toUsers == null || toUsers.Count == 0)
				throw new ArgumentException("toUsers must include at least one user.", "toUsers");
			var names = user.Name;
			foreach (var toUser in toUsers)
				names += ", " + toUser.Name;
			var now = DateTime.UtcNow;
			var pm = new PrivateMessage
			         	{
			         		Subject = TextParsingService.EscapeHtmlAndCensor(subject),
							UserNames = names,
							LastPostTime = now
			         	};
			pm.PMID = PrivateMessageRepository.CreatePrivateMessage(pm);
			PrivateMessageRepository.AddUsers(pm.PMID, new List<int> {user.UserID}, now, true);
			PrivateMessageRepository.AddUsers(pm.PMID, toUsers.Select(u => u.UserID).ToList(), now.AddSeconds(-1), false);
			var post = new PrivateMessagePost
			           	{
			           		FullText = TextParsingService.ForumCodeToHtml(fullText),
							Name = user.Name,
							PMID = pm.PMID,
							PostTime = now,
							UserID = user.UserID
			           	};
			PrivateMessageRepository.AddPost(post);
			return pm;
		}

		public void Reply(PrivateMessage pm, string fullText, User user)
		{
			if (pm == null || pm.PMID == 0)
				throw new ArgumentException("Can't reply to a PM that hasn't been persisted.", "pm");
			if (String.IsNullOrWhiteSpace(fullText))
				throw new ArgumentNullException("fullText");
			if (user == null)
				throw new ArgumentNullException("user");
			if (!IsUserInPM(user, pm))
				throw new Exception("Can't add a PM reply for a user not part of the PM.");
			var post = new PrivateMessagePost
			           	{
			           		FullText = TextParsingService.ForumCodeToHtml(fullText),
			           		Name = user.Name,
			           		PMID = pm.PMID,
			           		PostTime = DateTime.UtcNow,
			           		UserID = user.UserID
			           	};
			PrivateMessageRepository.AddPost(post);
			var users = PrivateMessageRepository.GetUsers(pm.PMID);
			foreach (var u in users)
				PrivateMessageRepository.SetArchive(pm.PMID, u.UserID, false);
			var now = DateTime.UtcNow;
			PrivateMessageRepository.UpdateLastPostTime(pm.PMID, now);
			PrivateMessageRepository.SetLastViewTime(pm.PMID, user.UserID, now);
		}

		public bool IsUserInPM(User user, PrivateMessage pm)
		{
			var pmUsers = PrivateMessageRepository.GetUsers(pm.PMID);
			return (pmUsers.Where(p => p.UserID == user.UserID).Count() != 0);
		}

		public void MarkPMRead(User user, PrivateMessage pm)
		{
			PrivateMessageRepository.SetLastViewTime(pm.PMID, user.UserID, DateTime.UtcNow);
		}

		public void Archive(User user, PrivateMessage pm)
		{
			PrivateMessageRepository.SetArchive(pm.PMID, user.UserID, true);
		}

		public void Unarchive(User user, PrivateMessage pm)
		{
			PrivateMessageRepository.SetArchive(pm.PMID, user.UserID, false);
		}
	}
}
