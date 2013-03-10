using System;
using PopForums.Models;

namespace PopForums.Services
{
	public interface ILastReadService
	{
		void MarkForumRead(User user, Forum forum);
		void MarkAllForumsRead(User user);
		void MarkTopicRead(User user, Topic topic);
		void GetForumReadStatus(User user, CategorizedForumContainer container);
		void GetTopicReadStatus(User user, PagedTopicContainer container);
		Post GetFirstUnreadPost(User user, Topic topic);
		DateTime? GetTopicReadStatus(User user, Topic topic);
		DateTime? GetForumReadStatus(User user, Forum forum);
	}
}