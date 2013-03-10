using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Services
{
	public interface IForumService
	{
		Forum Get(int forumID);
		Forum Get(string urlName);
		Forum Create(int? categoryID, string title, string description, bool isVisible, bool isArchived, int sortOrder, string forumAdapterName);
		void UpdateLast(Forum forum);
		void UpdateLast(Forum forum, DateTime lastTime, string lastName);
		void UpdateCounts(Forum forum);
		void IncrementPostCount(Forum forum);
		void IncrementPostAndTopicCount(Forum forum);
		CategorizedForumContainer GetCategorizedForumContainer();
		CategorizedForumContainer GetCategorizedForumContainerFilteredForUser(User user);
		List<int> GetNonViewableForumIDs(User user);
		ForumPermissionContext GetPermissionContext(Forum forum, User user);
		ForumPermissionContext GetPermissionContext(Forum forum, User user, Topic topic);
		Topic PostNewTopic(Forum forum, User user, ForumPermissionContext permissionContext, NewPost newPost, string ip, string userUrl, Func<Topic, string> topicLinkGenerator);
		void Update(Forum forum, int? categoryID, string title, string description, bool isVisible, bool isArchived, string forumAdapterName);
		void MoveForumUp(Forum forum);
		void MoveForumDown(Forum forum);
		List<string> GetForumPostRoles(Forum forum);
		List<string> GetForumViewRoles(Forum forum);
		void AddPostRole(Forum forum, string role);
		void RemovePostRole(Forum forum, string role);
		void AddViewRole(Forum forum, string role);
		void RemoveViewRole(Forum forum, string role);
		void RemoveAllPostRoles(Forum forum);
		void RemoveAllViewRoles(Forum forum);
		Dictionary<int, string> GetAllForumTitles();
		List<Topic> GetRecentTopics(User user, bool includeDeleted, int pageIndex, out PagerContext pagerContext);
		int GetAggregateTopicCount();
		int GetAggregatePostCount();
		List<int> GetViewableForumIDsFromViewRestrictedForums(User user);
	}
}