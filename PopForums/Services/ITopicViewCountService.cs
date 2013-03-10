using System.Web;
using PopForums.Models;

namespace PopForums.Services
{
	public interface ITopicViewCountService
	{
		void ProcessView(Topic topic, HttpContextBase context);
		void SetViewedTopic(Topic topic, HttpContextBase context);
	}
}