namespace PopForums.Services.Interfaces;

public interface ITopicViewCountService
{
    Task ProcessView(Topic topic);
    void SetViewedTopic(Topic topic);
}