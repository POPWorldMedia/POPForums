using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using PopForums.ExternalLogin;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using Xunit;

namespace PopForums.Test.ExternalLogin
{
	public class ExternalUserAssociationManagerTests
	{
		private ExternalUserAssociationManager GetManager()
		{
			_externalUserAssociationRepo = new Mock<IExternalUserAssociationRepository>();
			_userRepo = new Mock<IUserRepository>();
			_securityLogService = new Mock<ISecurityLogService>();
			return new ExternalUserAssociationManager(_externalUserAssociationRepo.Object, _userRepo.Object, _securityLogService.Object);
		}

		private Mock<IExternalUserAssociationRepository> _externalUserAssociationRepo;
		private Mock<IUserRepository> _userRepo;
		private Mock<ISecurityLogService> _securityLogService;

		[Fact]
		public async Task ExternalUserAssociationCheckThrowsWithNullArg()
		{
			var manager = GetManager();

			await Assert.ThrowsAsync<ArgumentNullException>(async () => await manager.ExternalUserAssociationCheck(null, ""));
		}

		[Fact]
		public async Task ExternalUserAssociationCheckResultFalseWithNullsIfNoMatchingAssociation()
		{
			var manager = GetManager();
			_externalUserAssociationRepo.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((ExternalUserAssociation) null);

			var result = await manager.ExternalUserAssociationCheck(new ExternalLoginInfo("", "", ""), "");

			Assert.False(result.Successful);
			Assert.Null(result.ExternalUserAssociation);
			Assert.Null(result.User);
		}

		[Fact]
		public async Task ExternalUserAssociationCheckResultFalseWithNullsIfNoMatchingUser()
		{
			var manager = GetManager();
			_externalUserAssociationRepo.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ExternalUserAssociation());
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).ReturnsAsync((User) null);

			var result = await manager.ExternalUserAssociationCheck(new ExternalLoginInfo("", "", ""), "");

			Assert.False(result.Successful);
			Assert.Null(result.ExternalUserAssociation);
			Assert.Null(result.User);
		}

		[Fact]
		public async Task ExternalUserAssociationCheckResultTrueWithHydratedResultIfMatchingAssociationAndUser()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation { Issuer = "Google", UserID = 123, ProviderKey = "abc"};
			var user = new User {UserID = association.UserID};
			_externalUserAssociationRepo.Setup(x => x.Get(association.Issuer, association.ProviderKey)).ReturnsAsync(association);
			_userRepo.Setup(x => x.GetUser(association.UserID)).ReturnsAsync(user);
			var authResult = new ExternalLoginInfo("Google", "abc", "");

			var result = await manager.ExternalUserAssociationCheck(authResult, "");

			Assert.True(result.Successful);
			Assert.Same(user, result.User);
			Assert.Same(association, result.ExternalUserAssociation);
		}

		[Fact]
		public async Task ExternalUserAssociationCheckResultTrueCallsSecurityLog()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation { Issuer = "Google", UserID = 123, ProviderKey = "abc" };
			var user = new User {UserID = association.UserID};
			_externalUserAssociationRepo.Setup(x => x.Get(association.Issuer, association.ProviderKey)).ReturnsAsync(association);
			_userRepo.Setup(x => x.GetUser(association.UserID)).ReturnsAsync(user);
			const string ip = "1.1.1.1";
			var authResult = new ExternalLoginInfo("Google", "abc", "");

			await manager.ExternalUserAssociationCheck(authResult, ip);

			_securityLogService.Verify(x => x.CreateLogEntry(user, user, ip, It.IsAny<string>(), SecurityLogType.ExternalAssociationCheckSuccessful));
		}

		[Fact]
		public async Task ExternalUserAssociationCheckResultFalseNoMatchCallsSecurityLog()
		{
			var manager = GetManager();
			_externalUserAssociationRepo.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((ExternalUserAssociation)null);
			const string ip = "1.1.1.1";
			var authResult = new ExternalLoginInfo("Google", "abc", "");

			await manager.ExternalUserAssociationCheck(authResult, ip);

			_securityLogService.Verify(x => x.CreateLogEntry((int?)null, null, ip, It.IsAny<string>(), SecurityLogType.ExternalAssociationCheckFailed), Times.Once());
		}

		[Fact]
		public async Task ExternalUserAssociationCheckResultFalseNoUserCallsSecurityLog()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation { Issuer = "Google", UserID = 123, ProviderKey = "abc" };
			_externalUserAssociationRepo.Setup(x => x.Get(association.Issuer, association.ProviderKey)).ReturnsAsync(association);
			_userRepo.Setup(x => x.GetUser(association.UserID)).ReturnsAsync((User)null);
			const string ip = "1.1.1.1";
			var authResult = new ExternalLoginInfo("Google", "abc", "");

			await manager.ExternalUserAssociationCheck(authResult, ip);

			_securityLogService.Verify(x => x.CreateLogEntry((int?)null, null, ip, It.IsAny<string>(), SecurityLogType.ExternalAssociationCheckFailed), Times.Once());
		}

		[Fact]
		public async Task AssociateThrowsWithNullUser()
		{
			var manager = GetManager();

			await Assert.ThrowsAsync<ArgumentNullException>(async () => await manager.Associate(null, It.IsAny<ExternalLoginInfo>(), String.Empty));
		}

		[Fact]
		public async Task AssociateNeverCallsRepoWithNullExternalAuthResult()
		{
			var manager = GetManager();

			await manager.Associate(new User(), null, String.Empty);

			_externalUserAssociationRepo.Verify(x => x.Save(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public async Task AssociateThrowsWithNoProviderKey()
		{
			var manager = GetManager();

			await Assert.ThrowsAsync<NullReferenceException>(async () => await manager.Associate(new User(), new ExternalLoginInfo("efwef", "", ""), string.Empty));
		}

		[Fact]
		public async Task AssociateMapsObjectsToRepoCall()
		{
			var manager = GetManager();
			var user = new User {UserID = 123};
			var externalAuthResult = new ExternalLoginInfo("wegggw", "wfweg", "wewg");

			await manager.Associate(user, externalAuthResult, String.Empty);

			_externalUserAssociationRepo.Verify(x => x.Save(user.UserID, externalAuthResult.LoginProvider, externalAuthResult.ProviderKey, externalAuthResult.ProviderDisplayName), Times.Once());
		}

		[Fact]
		public async Task AssociateSuccessCallsSecurityLog()
		{
			var manager = GetManager();
			var user = new User { UserID = 123 };
			var externalAuthResult = new ExternalLoginInfo("wegggw", "wfweg", "wewg");
			const string ip = "1.1.1.1";

			await manager.Associate(user, externalAuthResult, ip);

			_securityLogService.Verify(x => x.CreateLogEntry(user, user, ip, It.IsAny<string>(), SecurityLogType.ExternalAssociationSet), Times.Once());
		}

		[Fact]
		public async Task GetExternalUserAssociationsCallsRepoByUserID()
		{
			var manager = GetManager();
			var user = new User { UserID = 123 };

			await manager.GetExternalUserAssociations(user);

			_externalUserAssociationRepo.Verify(x => x.GetByUser(user.UserID), Times.Once());
		}

		[Fact]
		public async Task GetExternalUserAssociationsReturnsCollectionFromRepo()
		{
			var manager = GetManager();
			var user = new User { UserID = 123 };
			var collection = new List<ExternalUserAssociation>();
			_externalUserAssociationRepo.Setup(x => x.GetByUser(user.UserID)).ReturnsAsync(collection);

			var result = await manager.GetExternalUserAssociations(user);

			Assert.Same(collection, result);
		}

		[Fact]
		public async Task RemoveAssociationNeverCallsRepoIfNoAssociationIsFound()
		{
			var manager = GetManager();
			_externalUserAssociationRepo.Setup(x => x.Get(It.IsAny<int>())).ReturnsAsync((ExternalUserAssociation) null);

			await manager.RemoveAssociation(new User(), 4556, String.Empty);

			_externalUserAssociationRepo.Verify(x => x.Delete(It.IsAny<int>()), Times.Never());
		}

		[Fact]
		public async Task RemoveAssociationLogsTheRemoval()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation {ExternalUserAssociationID = 123, Issuer = "Google", Name = "Jeffy", ProviderKey = "oihfoihfef", UserID = 456};
			var user = new User {UserID = association.UserID};
			const string ip = "1.1.1.1";
			_externalUserAssociationRepo.Setup(x => x.Get(association.ExternalUserAssociationID)).ReturnsAsync(association);

			await manager.RemoveAssociation(user, association.ExternalUserAssociationID, ip);

			_securityLogService.Verify(x => x.CreateLogEntry(user, user, ip, It.IsAny<string>(), SecurityLogType.ExternalAssociationRemoved), Times.Once());
		}

		[Fact]
		public async Task RemoveAssociationThrowsIfUserIDsDontMatch()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation { ExternalUserAssociationID = 123, UserID = 456 };
			var user = new User { UserID = 789 };
			_externalUserAssociationRepo.Setup(x => x.Get(association.ExternalUserAssociationID)).ReturnsAsync(association);

			await Assert.ThrowsAsync<Exception>(async () => await manager.RemoveAssociation(user, association.ExternalUserAssociationID, string.Empty));
		}

		[Fact]
		public async Task RemoveAssociationCallsRepoOnSuccessfulMatch()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation { ExternalUserAssociationID = 123, UserID = 456 };
			var user = new User { UserID = association.UserID };
			_externalUserAssociationRepo.Setup(x => x.Get(association.ExternalUserAssociationID)).ReturnsAsync(association);

			await manager.RemoveAssociation(user, association.ExternalUserAssociationID, String.Empty);

			_externalUserAssociationRepo.Verify(x => x.Delete(association.ExternalUserAssociationID), Times.Once());
		}

		[Fact]
		public async Task GetExternalUserAssociationsThrowsIfAssociationDoesntMatchUser()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation { ExternalUserAssociationID = 456, UserID = 789};
			var user = new User();
			_externalUserAssociationRepo.Setup(x => x.Get(association.ExternalUserAssociationID)).ReturnsAsync(association);

			await Assert.ThrowsAsync<Exception>(async () => await manager.RemoveAssociation(user, association.ExternalUserAssociationID, string.Empty));
		}

		[Fact]
		public async Task GetExternalUserAssociationsCallsRepoWithMatchingUserIDs()
		{
			var manager = GetManager();
			var user = new User { UserID = 123 };
			var association = new ExternalUserAssociation { ExternalUserAssociationID = 456, UserID = user.UserID };
			_externalUserAssociationRepo.Setup(x => x.Get(association.ExternalUserAssociationID)).ReturnsAsync(association);

			await manager.RemoveAssociation(user, association.ExternalUserAssociationID, String.Empty);

			_externalUserAssociationRepo.Verify(x => x.Delete(association.ExternalUserAssociationID), Times.Once());
		}
	}
}