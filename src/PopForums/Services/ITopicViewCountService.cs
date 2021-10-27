namespace PopForums.Services;

public interface ITopicViewCountService
{
	Task ProcessView(Topic topic);
	void SetViewedTopic(Topic topic);
}