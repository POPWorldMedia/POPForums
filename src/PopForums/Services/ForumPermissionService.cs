using System.Linq;
using System.Threading.Tasks;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IForumPermissionService
	{
		Task<ForumPermissionContext> GetPermissionContext(Forum forum, User user);
		Task<ForumPermissionContext> GetPermissionContext(Forum forum, User user, Topic topic);
	}

	public class ForumPermissionService : IForumPermissionService
	{
		private readonly IForumRepository _forumRepository;

		public ForumPermissionService(IForumRepository forumRepository)
		{
			_forumRepository = forumRepository;
		}

		public async Task<ForumPermissionContext> GetPermissionContext(Forum forum, User user)
		{
			return await GetPermissionContext(forum, user, null);
		}

		public async Task<ForumPermissionContext> GetPermissionContext(Forum forum, User user, Topic topic)
		{
			var context = new ForumPermissionContext { DenialReason = string.Empty };
			var viewRestrictionRoles = await _forumRepository.GetForumViewRoles(forum.ForumID);
			var postRestrictionRoles = await _forumRepository.GetForumPostRoles(forum.ForumID);

			// view
			if (viewRestrictionRoles.Count == 0)
				context.UserCanView = true;
			else
			{
				context.UserCanView = false;
				if (user != null && viewRestrictionRoles.Where(user.IsInRole).Any())
					context.UserCanView = true;
			}

			// post
			if (user == null || !context.UserCanView)
			{
				context.UserCanPost = false;
				context.DenialReason = Resources.LoginToPost;
			}
			else
				if (!user.IsApproved)
			{
				context.DenialReason += "You can't post until you have verified your account. ";
				context.UserCanPost = false;
			}
			else
			{
				if (postRestrictionRoles.Count == 0)
					context.UserCanPost = true;
				else
				{
					if (postRestrictionRoles.Where(user.IsInRole).Any())
						context.UserCanPost = true;
					else
					{
						context.DenialReason += Resources.ForumNoPost + ". ";
						context.UserCanPost = false;
					}
				}
			}

			if (topic != null && topic.IsClosed)
			{
				context.UserCanPost = false;
				context.DenialReason = Resources.Closed + ". ";
			}

			if (topic != null && topic.IsDeleted)
			{
				if (user == null || !user.IsInRole(PermanentRoles.Moderator))
					context.UserCanView = false;
				context.DenialReason += "Topic is deleted. ";
			}

			if (forum.IsArchived)
			{
				context.UserCanPost = false;
				context.DenialReason += Resources.Archived + ". ";
			}

			// moderate
			context.UserCanModerate = false;
			if (user != null && (user.IsInRole(PermanentRoles.Admin) || user.IsInRole(PermanentRoles.Moderator)))
				context.UserCanModerate = true;

			return context;
		}
	}
}