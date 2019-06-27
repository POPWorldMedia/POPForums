using PopForums.Models;

namespace PopForums.Services
{
	public interface ITopicViewCountService
	{
		void ProcessView(Topic topic);
		void SetViewedTopic(Topic topic);
	}
}