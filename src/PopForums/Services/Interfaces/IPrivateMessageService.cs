namespace PopForums.Services.Interfaces;

public interface IPrivateMessageService
{
    Task<PrivateMessage> Get(int pmID, int userID);

    Task<List<PrivateMessagePost>> GetMostRecentPosts(int pmID, DateTime afterDateTime);

    Task<List<PrivateMessagePost>> GetPosts(int pmID, DateTime beforeDateTime);

    Task<Tuple<List<PrivateMessage>, PagerContext>> GetPrivateMessages(User user, PrivateMessageBoxType boxType, int pageIndex);

    Task<int> GetUnreadCount(int userID);

    Task<PrivateMessage> Create(string fullText, User user, List<User> toUsers);

    Task Reply(PrivateMessage pm, string fullText, User user);

    Task<bool> IsUserInPM(int userID, int pmID);

    Task MarkPMRead(int userID, int pmID);

    Task Archive(User user, PrivateMessage pm);

    Task Unarchive(User user, PrivateMessage pm);

    Task<int> GetFirstUnreadPostID(int pmID, DateTime lastViewDate);

    Task<List<PrivateMessageUser>> GetUsers(int pmID);
}
