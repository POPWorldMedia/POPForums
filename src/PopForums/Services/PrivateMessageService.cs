using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IPrivateMessageService
	{
		Task<PrivateMessage> Get(int pmID);
		Task<List<PrivateMessagePost>> GetPosts(PrivateMessage pm);
		Task<Tuple<List<PrivateMessage>, PagerContext>> GetPrivateMessages(User user, PrivateMessageBoxType boxType, int pageIndex);
		Task<int> GetUnreadCount(User user);
		Task<PrivateMessage> Create(string subject, string fullText, User user, List<User> toUsers);
		Task Reply(PrivateMessage pm, string fullText, User user);
		Task<bool> IsUserInPM(User user, PrivateMessage pm);
		Task MarkPMRead(User user, PrivateMessage pm);
		Task Archive(User user, PrivateMessage pm);
		Task Unarchive(User user, PrivateMessage pm);
	}

	public class PrivateMessageService : IPrivateMessageService
	{
		public PrivateMessageService(IPrivateMessageRepository privateMessageRepo, ISettingsManager settingsManager, ITextParsingService textParsingService)
		{
			_privateMessageRepository = privateMessageRepo;
			_settingsManager = settingsManager;
			_textParsingService = textParsingService;
		}

		private readonly IPrivateMessageRepository _privateMessageRepository;
		private readonly ISettingsManager _settingsManager;
		private readonly ITextParsingService _textParsingService;

		public async Task<PrivateMessage> Get(int pmID)
		{
			return await _privateMessageRepository.Get(pmID);
		}

		public async Task<List<PrivateMessagePost>> GetPosts(PrivateMessage pm)
		{
			return await _privateMessageRepository.GetPosts(pm.PMID);
		}

		public async Task<Tuple<List<PrivateMessage>, PagerContext>> GetPrivateMessages(User user, PrivateMessageBoxType boxType, int pageIndex)
		{
			var total = await _privateMessageRepository.GetBoxCount(user.UserID, boxType);
			var pageSize = _settingsManager.Current.TopicsPerPage;
			var startRow = ((pageIndex - 1) * pageSize) + 1;
			var totalPages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(total) / Convert.ToDouble(pageSize)));
			var pagerContext = new PagerContext { PageCount = totalPages, PageIndex = pageIndex, PageSize = pageSize };
			var messages = await _privateMessageRepository.GetPrivateMessages(user.UserID, boxType, startRow, pageSize);
			return Tuple.Create(messages, pagerContext);
		}

		public async Task<int> GetUnreadCount(User user)
		{
			return await _privateMessageRepository.GetUnreadCount(user.UserID);
		}

		public async Task<PrivateMessage> Create(string subject, string fullText, User user, List<User> toUsers)
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
			         		Subject = _textParsingService.EscapeHtmlAndCensor(subject),
							UserNames = names,
							LastPostTime = now
			         	};
			pm.PMID = await _privateMessageRepository.CreatePrivateMessage(pm);
			await _privateMessageRepository.AddUsers(pm.PMID, new List<int> {user.UserID}, now, true);
			await _privateMessageRepository.AddUsers(pm.PMID, toUsers.Select(u => u.UserID).ToList(), now.AddSeconds(-1), false);
			var post = new PrivateMessagePost
			           	{
			           		FullText = _textParsingService.ForumCodeToHtml(fullText),
							Name = user.Name,
							PMID = pm.PMID,
							PostTime = now,
							UserID = user.UserID
			           	};
			await _privateMessageRepository.AddPost(post);
			return pm;
		}

		public async Task Reply(PrivateMessage pm, string fullText, User user)
		{
			if (pm == null || pm.PMID == 0)
				throw new ArgumentException("Can't reply to a PM that hasn't been persisted.", "pm");
			if (fullText == null)
				throw new ArgumentNullException("fullText");
			if (user == null)
				throw new ArgumentNullException("user");
			if (await IsUserInPM(user, pm) == false)
				throw new Exception("Can't add a PM reply for a user not part of the PM.");
			var post = new PrivateMessagePost
			           	{
			           		FullText = _textParsingService.ForumCodeToHtml(fullText),
			           		Name = user.Name,
			           		PMID = pm.PMID,
			           		PostTime = DateTime.UtcNow,
			           		UserID = user.UserID
			           	};
			await _privateMessageRepository.AddPost(post);
			var users = await _privateMessageRepository.GetUsers(pm.PMID);
			foreach (var u in users)
				await _privateMessageRepository.SetArchive(pm.PMID, u.UserID, false);
			var now = DateTime.UtcNow;
			await _privateMessageRepository.UpdateLastPostTime(pm.PMID, now);
			await _privateMessageRepository.SetLastViewTime(pm.PMID, user.UserID, now);
		}

		public async Task<bool> IsUserInPM(User user, PrivateMessage pm)
		{
			var pmUsers = await _privateMessageRepository.GetUsers(pm.PMID);
			return pmUsers.Count(p => p.UserID == user.UserID) != 0;
		}

		public async Task MarkPMRead(User user, PrivateMessage pm)
		{
			await _privateMessageRepository.SetLastViewTime(pm.PMID, user.UserID, DateTime.UtcNow);
		}

		public async Task Archive(User user, PrivateMessage pm)
		{
			await _privateMessageRepository.SetArchive(pm.PMID, user.UserID, true);
		}

		public async Task Unarchive(User user, PrivateMessage pm)
		{
			await _privateMessageRepository.SetArchive(pm.PMID, user.UserID, false);
		}
	}
}
