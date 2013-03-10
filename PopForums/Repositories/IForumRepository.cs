using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IForumRepository
	{
		Forum Get(int forumID);
		Forum Get(string urlName);
		Forum Create(int? categoryID, string title, string description, bool isVisible, bool isArchived, int sortOrder, string urlName, string forumAdapterName);
		List<Forum> GetForumsInCategory(int? categoryID);
		List<string> GetUrlNamesThatStartWith(string urlName);
		void Update(int forumID, int? categoryID, string title, string description, bool isVisible, bool isArchived, string urlName, string forumAdapterName);
		void UpdateSortOrder(int forumID, int newSortOrder);
		void UpdateCategoryAssociation(int forumID, int? categoryID);
		void UpdateLastTimeAndUser(int forumID, DateTime lastTime, string lastName);
		void UpdateTopicAndPostCounts(int forumID, int topicCount, int postCount);
		void IncrementPostCount(int forumID);
		void IncrementPostAndTopicCount(int forumID);
		IEnumerable<Forum> GetAll();
		IEnumerable<Forum> GetAllVisible();

		/// <summary>
		/// Gets role requirements for the ability to post in this forum.
		/// </summary>
		/// <remarks>This should generally be cached, because the calling code runs this test on every forum when displayed.</remarks>
		/// <param name="forumID">ID of forum to fetch posting roles for.</param>
		/// <returns>A List of strings for roles required for posting.</returns>
		List<string> GetForumPostRoles(int forumID);

		/// <summary>
		/// Gets role requirements for the ability to view this forum.
		/// </summary>
		/// <remarks>This should generally be cached, because the calling code runs this test on every forum when displayed.</remarks>
		/// <param name="forumID">ID of forum to fetch roles required for viewing.</param>
		/// <returns>A List of strings for roles required for viewing.</returns>
		List<string> GetForumViewRoles(int forumID);

		/// <summary>
		/// Gets a graph of forums and associated post restrictions.
		/// </summary>
		/// <remarks>This should generally be cached, because the calling code runs this test on every forum when displayed.</remarks>
		/// <returns>A dictionary of key/value pairs of forums and post role restrictions.</returns>
		Dictionary<int, List<string>> GetForumPostRestrictionRoleGraph();

		/// <summary>
		/// Gets a graph of forums and associated view restrictions.
		/// </summary>
		/// <remarks>This should generally be cached, because the calling code runs this test on every forum when displayed.</remarks>
		/// <returns>A dictionary of key/value pairs of forums and view role restrictions.</returns>
		Dictionary<int, List<string>> GetForumViewRestrictionRoleGraph();

		void AddPostRole(int forumID, string role);
		void RemovePostRole(int forumID, string role);
		void AddViewRole(int forumID, string role);
		void RemoveViewRole(int forumID, string role);
		void RemoveAllPostRoles(int forumID);
		void RemoveAllViewRoles(int forumID);

		/// <summary>
		/// Gets all UrlName values for all forums.
		/// </summary>
		/// <remarks>This should generally be cached, as it's used as a lookup for URL routing on every request.</remarks>
		/// <returns>An enumerable object of strings.</returns>
		IEnumerable<string> GetAllForumUrlNames();

		Dictionary<int, string> GetAllForumTitles();
		int GetAggregateTopicCount();
		int GetAggregatePostCount();
	}
}
