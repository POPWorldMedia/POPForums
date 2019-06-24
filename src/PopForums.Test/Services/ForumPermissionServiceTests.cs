using System.Collections.Generic;
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
		public void NoViewRestrictionWithUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, GetUser());
			Assert.True(permission.UserCanView);
			Assert.Empty(permission.DenialReason);
		}

		[Fact]
		public void NoViewRestrictionWithoutUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, null);
			Assert.True(permission.UserCanView);
		}

		[Fact]
		public void ViewRestrictionUserNotInRole()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string> { "blah" });
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, GetUser());
			Assert.False(permission.UserCanView);
		}

		[Fact]
		public void ViewRestrictionUserCantPostEither()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string> { "blah" });
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, GetUser());
			Assert.False(permission.UserCanView);
			Assert.False(permission.UserCanPost);
		}

		[Fact]
		public void ViewRestrictionNoUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string> { "blah" });
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, null);
			Assert.False(permission.UserCanView);
		}

		[Fact]
		public void ViewRestrictionUserInRole()
		{
			var user = GetUser();
			user.Roles.Add("blah");
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string> { "blah" });
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.True(permission.UserCanView);
		}

		[Fact]
		public void PostRestrictionNoUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string> { "blah" });
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, null);
			Assert.False(permission.UserCanPost);
		}

		[Fact]
		public void PostRestrictionUserInRole()
		{
			var user = GetUser();
			user.Roles.Add("blah");
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string> { "blah" });
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.True(permission.UserCanPost);
			Assert.Empty(permission.DenialReason);
		}

		[Fact]
		public void PostRestrictionUserNotApproved()
		{
			var user = GetUser();
			user.IsApproved = false;
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.False(permission.UserCanPost);
			Assert.NotEmpty(permission.DenialReason);
		}

		[Fact]
		public void PostRestrictionUserNotInRole()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string> { "blah" });
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, GetUser());
			Assert.False(permission.UserCanPost);
			Assert.NotEmpty(permission.DenialReason);
		}

		[Fact]
		public void ModerateNoUser()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, null);
			Assert.False(permission.UserCanModerate);
		}

		[Fact]
		public void ModerateUserIsAdmin()
		{
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Admin);
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.True(permission.UserCanModerate);
		}

		[Fact]
		public void ModerateUserIsModerator()
		{
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var permission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user);
			Assert.True(permission.UserCanModerate);
		}

		[Fact]
		public void TopicClosed()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user, new Topic { TopicID = 4, IsClosed = true });
			Assert.False(premission.UserCanPost);
		}

		[Fact]
		public void TopicOpen()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user, new Topic { TopicID = 4, IsClosed = false });
			Assert.True(premission.UserCanPost);
		}

		[Fact]
		public void WithUserTopicDeleted()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user, new Topic { TopicID = 4, IsDeleted = true });
			Assert.False(premission.UserCanView);
		}

		[Fact]
		public void AnonTopicDeleted()
		{
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, null, new Topic { TopicID = 4, IsDeleted = true });
			Assert.False(premission.UserCanView);
		}

		[Fact]
		public void ModOnTopicDeleted()
		{
			var user = GetUser();
			user.Roles.Add(PermanentRoles.Moderator);
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1 }, user, new Topic { TopicID = 4, IsDeleted = true });
			Assert.True(premission.UserCanView);
		}

		[Fact]
		public void ForumNotArchived()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1, IsArchived = false }, user, new Topic { TopicID = 4 });
			Assert.True(premission.UserCanPost);
		}

		[Fact]
		public void ForumIsArchived()
		{
			var user = GetUser();
			var forumService = GetService();
			_mockForumRepo.Setup(f => f.GetForumPostRoles(1)).Returns(new List<string>());
			_mockForumRepo.Setup(f => f.GetForumViewRoles(1)).Returns(new List<string>());
			var premission = forumService.GetPermissionContext(new Forum { ForumID = 1, IsArchived = true }, user, new Topic { TopicID = 4 });
			Assert.False(premission.UserCanPost);
		}
	}
}