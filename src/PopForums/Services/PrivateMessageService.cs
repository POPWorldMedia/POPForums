namespace PopForums.Services;

public interface IPrivateMessageService
{
	Task<PrivateMessage> Get(int pmID, int userID);
	Task<List<PrivateMessagePost>> GetPosts(int pmID);
	Task<Tuple<List<PrivateMessage>, PagerContext>> GetPrivateMessages(User user, PrivateMessageBoxType boxType, int pageIndex);
	Task<int> GetUnreadCount(User user);
	Task<PrivateMessage> Create(string fullText, User user, List<User> toUsers);
	Task Reply(PrivateMessage pm, string fullText, User user);
	Task<bool> IsUserInPM(int userID, int pmID);
	Task MarkPMRead(int userID, int pmID);
	Task Archive(User user, PrivateMessage pm);
	Task Unarchive(User user, PrivateMessage pm);
	Task<int?> GetFirstUnreadPostID(int pmID, DateTime lastViewDate);
}

public class PrivateMessageService : IPrivateMessageService
{
	public PrivateMessageService(IPrivateMessageRepository privateMessageRepo, ISettingsManager settingsManager, ITextParsingService textParsingService, IBroker broker)
	{
		_privateMessageRepository = privateMessageRepo;
		_settingsManager = settingsManager;
		_textParsingService = textParsingService;
		_broker = broker;
	}

	private readonly IPrivateMessageRepository _privateMessageRepository;
	private readonly ISettingsManager _settingsManager;
	private readonly ITextParsingService _textParsingService;
	private readonly IBroker _broker;

	public async Task<PrivateMessage> Get(int pmID, int userID)
	{
		return await _privateMessageRepository.Get(pmID, userID);
	}

	public async Task<List<PrivateMessagePost>> GetPosts(int pmID)
	{
		return await _privateMessageRepository.GetPosts(pmID);
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

	public async Task<PrivateMessage> Create(string fullText, User user, List<User> toUsers)
	{
		if (String.IsNullOrWhiteSpace(fullText))
			throw new ArgumentNullException(nameof(fullText));
		if (user == null)
			throw new ArgumentNullException(nameof(user));
		if (toUsers == null || toUsers.Count == 0)
			throw new ArgumentException("toUsers must include at least one user.", nameof(toUsers));
		var userIDs = toUsers.Select(x => x.UserID).ToList();
		userIDs.Add(user.UserID);
		var existingPMID = await _privateMessageRepository.GetExistingFromIDs(userIDs);
		if (existingPMID != 0)
		{
			var existingPM = await _privateMessageRepository.Get(existingPMID, user.UserID);
			await Reply(existingPM, fullText, user);
			return existingPM;
		}
		var now = DateTime.UtcNow;
		var dynamicUserList = toUsers.Select(x => new {x.UserID, x.Name}).ToList();
		dynamicUserList.Add(new {user.UserID, user.Name});
		var serializedUsers = JsonSerializer.SerializeToElement(dynamicUserList, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
		var pm = new PrivateMessage
		{
			Users = serializedUsers,
			LastPostTime = now
		};
		pm.PMID = await _privateMessageRepository.CreatePrivateMessage(pm);
		await _privateMessageRepository.AddUsers(pm.PMID, new List<int> {user.UserID}, now, false);
		await _privateMessageRepository.AddUsers(pm.PMID, toUsers.Select(u => u.UserID).ToList(), now.AddSeconds(-1), false);
		var post = new PrivateMessagePost
		{
			FullText = _textParsingService.ForumCodeToHtml(fullText),
			Name = user.Name,
			PMID = pm.PMID,
			PostTime = now,
			UserID = user.UserID
		};
		foreach (var receiver in toUsers)
		{
			var receiverPMCount = await _privateMessageRepository.GetUnreadCount(receiver.UserID);
			_broker.NotifyPMCount(receiver.UserID, receiverPMCount);
		}
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
		if (await IsUserInPM(user.UserID, pm.PMID) == false)
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
		_broker.SendPMMessage(post);
		foreach (var receiver in users)
		{
			var receiverPMCount = await _privateMessageRepository.GetUnreadCount(receiver.UserID);
			_broker.NotifyPMCount(receiver.UserID, receiverPMCount);
		}
	}

	public async Task<bool> IsUserInPM(int userID, int pmID)
	{
		var pmUsers = await _privateMessageRepository.GetUsers(pmID);
		return pmUsers.Count(p => p.UserID == userID) != 0;
	}

	public async Task MarkPMRead(int userID, int pmID)
	{
		await _privateMessageRepository.SetLastViewTime(pmID, userID, DateTime.UtcNow);
		var pmCount = await _privateMessageRepository.GetUnreadCount(userID);
		_broker.NotifyPMCount(userID, pmCount);
	}

	public async Task Archive(User user, PrivateMessage pm)
	{
		await _privateMessageRepository.SetArchive(pm.PMID, user.UserID, true);
	}

	public async Task Unarchive(User user, PrivateMessage pm)
	{
		await _privateMessageRepository.SetArchive(pm.PMID, user.UserID, false);
	}

	public async Task<int?> GetFirstUnreadPostID(int pmID, DateTime lastViewDate)
	{
		return await _privateMessageRepository.GetFirstUnreadPostID(pmID, lastViewDate);
	}
}