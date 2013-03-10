using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Services
{
	public interface IFavoriteTopicService
	{
		List<Topic> GetTopics(User user, int pageIndex, out PagerContext pagerContext);
		bool IsTopicFavorite(User user, Topic topic);
		void AddFavoriteTopic(User user, Topic topic);
		void RemoveFavoriteTopic(User user, Topic topic);
	}
}