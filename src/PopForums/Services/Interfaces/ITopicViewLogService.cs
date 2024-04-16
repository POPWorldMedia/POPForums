namespace PopForums.Services.Interfaces;

public interface ITopicViewLogService
{
    Task LogView(int userID, int topicID);
}
