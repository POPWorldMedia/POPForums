using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using Xunit;

namespace PopForums.Test.Services
{
	public class ForumPermissionServiceTests
	{
		private ForumPermissionService GetService()
		{
			_mockForumRepo = new Mock<IForumRepository>();
			return new ForumPermissionService(_mockForumRepo.Object);
		}

		private Mock<IForumRepository> _mockForumRepo;

		private User GetUser()
		{
			var user = Models.UserTest.GetTestUser();
			user.Roles = new List<string>();
			return user;
		}

		[Fact]
		public async Task NoViewRestrictionWithUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var permission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, GetUser());
			Assert.True(permission.UserCanView);
			Assert.Empty(permission.DenialReason);
		}

		[Fact]
		public async Task NoViewRestrictionWithoutUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var permission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, null);
			Assert.True(permission.UserCanView);
		}

		[Fact]
		public async Task ViewRestrictionUserNotInRole()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string> { "blah" });
			var permission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, GetUser());
			Assert.False(permission.UserCanView);
		}

		[Fact]
		public async Task ViewRestrictionUserCantPostEither()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string> { "blah" });
			var permission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, GetUser());
			Assert.False(permission.UserCanView);
			Assert.False(permission.UserCanPost);
		}

		[Fact]
		public async Task ViewRestrictionNoUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string> { "blah" });
			var permission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, null);
			Assert.False(permission.UserCanView);
		}

		[Fact]
		public async Task ViewRestrictionUserInRole()
		{
			var user = GetUser();
			user.Roles.Add("blah");
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string> { "blah" });
			var permission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.True(permission.UserCanView);
		}

		[Fact]
		public async Task PostRestrictionNoUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string> { "blah" });
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var permission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, null);
			Assert.False(permission.UserCanPost);
		}

		[Fact]
		public async Task PostRestrictionUserInRole()
		{
			var user = GetUser();
			user.Roles.Add("blah");
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string> { "blah" });
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var permission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.True(permission.UserCanPost);
			Assert.Empty(permission.DenialReason);
		}

		[Fact]
		public async Task PostRestrictionUserNotApproved()
		{
			var user = GetUser();
			user.IsApproved = false;
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var permission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.False(permission.UserCanPost);
			Assert.NotEmpty(permission.DenialReason);
		}

		[Fact]
		public async Task PostRestrictionUserNotInRole()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string> { "blah" });
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var permission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, GetUser());
			Assert.False(permission.UserCanPost);
			Assert.NotEmpty(permission.DenialReason);
		}

		[Fact]
		public async Task ModerateNoUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var permission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, null);
			Assert.False(permission.UserCanModerate);
		}

		[Fact]
		public async Task ModerateUserIsAdmin()
		{
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Admin);
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var permission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.True(permission.UserCanModerate);
		}

		[Fact]
		public async Task ModerateUserIsModerator()
		{
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var permission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.True(permission.UserCanModerate);
		}

		[Fact]
		public async Task TopicClosed()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var premission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, user, new Topic { TopicID = 4, IsClosed = true });
			Assert.False(premission.UserCanPost);
		}

		[Fact]
		public async Task TopicOpen()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var premission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, user, new Topic { TopicID = 4, IsClosed = false });
			Assert.True(premission.UserCanPost);
		}

		[Fact]
		public async Task WithUserTopicDeleted()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var premission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, user, new Topic { TopicID = 4, IsDeleted = true });
			Assert.False(premission.UserCanView);
		}

		[Fact]
		public async Task AnonTopicDeleted()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var premission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, null, new Topic { TopicID = 4, IsDeleted = true });
			Assert.False(premission.UserCanView);
		}

		[Fact]
		public async Task ModOnTopicDeleted()
		{
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var premission = await forumService.GetPermissionContext(new Forum { ForumID = 1 }, user, new Topic { TopicID = 4, IsDeleted = true });
			Assert.True(premission.UserCanView);
		}

		[Fact]
		public async Task ForumNotArchived()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var premission = await forumService.GetPermissionContext(new Forum { ForumID = 1, IsArchived = false }, user, new Topic { TopicID = 4 });
			Assert.True(premission.UserCanPost);
		}

		[Fact]
		public async Task ForumIsArchived()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).ReturnsAsync(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).ReturnsAsync(new List<string>());
			var premission = await forumService.GetPermissionContext(new Forum { ForumID = 1, IsArchived = true }, user, new Topic { TopicID = 4 });
			Assert.False(premission.UserCanPost);
		}
	}
}