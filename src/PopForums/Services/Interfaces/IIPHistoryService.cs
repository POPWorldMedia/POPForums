namespace PopForums.Services.Interfaces;

public interface IIPHistoryService
{
    Task<List<IPHistoryEvent>> GetHistory(string ip, DateTime start, DateTime end);
}
