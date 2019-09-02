using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Services
{
	public interface ITopicViewCountService
	{
		Task ProcessView(Topic topic);
		void SetViewedTopic(Topic topic);
	}
}