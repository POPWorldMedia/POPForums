namespace PopForums.Services.Interfaces;

public interface ISecurityLogService
{
    Task<List<SecurityLogEntry>> GetLogEntriesByUserID(int userID, DateTime startDate, DateTime endDate);

    Task<List<SecurityLogEntry>> GetLogEntriesByUserName(string name, DateTime startDate, DateTime endDate);

    Task CreateLogEntry(User user, User targetUser, string ip, string message, SecurityLogType securityLogType);

    Task CreateLogEntry(int userID, int targetUserID, string ip, string message, SecurityLogType securityLogType);

    Task CreateLogEntry(int userID, int targetUserID, string ip, string message, SecurityLogType securityLogType, DateTime timeStamp);

    Task<List<IPHistoryEvent>> GetIPHistory(string ip, DateTime start, DateTime end);
}
