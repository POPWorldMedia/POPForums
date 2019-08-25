using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IForumRepository
	{
		Task<Forum> Get(int forumID);
		Task<Forum> Get(string urlName);
		Task<Forum> Create(int? categoryID, string title, string description, bool isVisible, bool isArchived, int sortOrder, string urlName, string forumAdapterName, bool isQAForum);
		Task<List<Forum>> GetForumsInCategory(int? categoryID);
		Task<List<string>> GetUrlNamesThatStartWith(string urlName);
		Task Update(int forumID, int? categoryID, string title, string description, bool isVisible, bool isArchived, string urlName, string forumAdapterName, bool isQAForum);
		Task UpdateSortOrder(int forumID, int newSortOrder);
		Task UpdateCategoryAssociation(int forumID, int? categoryID);
		Task UpdateLastTimeAndUser(int forumID, DateTime lastTime, string lastName);
		Task UpdateTopicAndPostCounts(int forumID, int topicCount, int postCount);
		Task IncrementPostCount(int forumID);
		Task IncrementPostAndTopicCount(int forumID);
		Task<IEnumerable<Forum>> GetAll();
		Task<IEnumerable<Forum>> GetAllVisible();

		/// <summary>
		/// Gets role requirements for the ability to post in this forum.
		/// </summary>
		/// <remarks>This should generally be cached, because the calling code runs this test on every forum when displayed.</remarks>
		/// <param name="forumID">ID of forum to fetch posting roles for.</param>
		/// <returns>A List of strings for roles required for posting.</returns>
		Task<List<string>> GetForumPostRoles(int forumID);

		/// <summary>
		/// Gets role requirements for the ability to view this forum.
		/// </summary>
		/// <remarks>This should generally be cached, because the calling code runs this test on every forum when displayed.</remarks>
		/// <param name="forumID">ID of forum to fetch roles required for viewing.</param>
		/// <returns>A List of strings for roles required for viewing.</returns>
		Task<List<string>> GetForumViewRoles(int forumID);

		/// <summary>
		/// Gets a graph of forums and associated post restrictions.
		/// </summary>
		/// <remarks>This should generally be cached, because the calling code runs this test on every forum when displayed.</remarks>
		/// <returns>A dictionary of key/value pairs of forums and post role restrictions.</returns>
		Task<Dictionary<int, List<string>>> GetForumPostRestrictionRoleGraph();

		/// <summary>
		/// Gets a graph of forums and associated view restrictions.
		/// </summary>
		/// <remarks>This should generally be cached, because the calling code runs this test on every forum when displayed.</remarks>
		/// <returns>A dictionary of key/value pairs of forums and view role restrictions.</returns>
		Task<Dictionary<int, List<string>>> GetForumViewRestrictionRoleGraph();

		Task AddPostRole(int forumID, string role);
		Task RemovePostRole(int forumID, string role);
		Task AddViewRole(int forumID, string role);
		Task RemoveViewRole(int forumID, string role);
		Task RemoveAllPostRoles(int forumID);
		Task RemoveAllViewRoles(int forumID);

		/// <summary>
		/// Gets all UrlName values for all forums.
		/// </summary>
		/// <remarks>This should generally be cached, as it's used as a lookup for URL routing on every request.</remarks>
		/// <returns>An enumerable object of strings.</returns>
		Task<IEnumerable<string>> GetAllForumUrlNames();

		Dictionary<int, string> GetAllForumTitles();
		Task<int> GetAggregateTopicCount();
		Task<int> GetAggregatePostCount();
	}
}
