namespace PopForums.Repositories;

public interface ISecurityLogRepository
{
	Task Create(SecurityLogEntry logEntry);
	Task<List<SecurityLogEntry>> GetByUserID(int userID, DateTime startDate, DateTime endDate);
	Task<List<IPHistoryEvent>> GetIPHistory(string ip, DateTime start, DateTime end);
}