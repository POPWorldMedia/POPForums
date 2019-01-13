using System;
using System.Collections.Generic;
using System.Security.Claims;
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
		public void ExternalUserAssociationCheckThrowsWithNullArg()
		{
			var manager = GetManager();

			Assert.Throws<ArgumentNullException>(() => manager.ExternalUserAssociationCheck(null, ""));
		}

		[Fact]
		public void ExternalUserAssociationCheckResultFalseWithNullsIfNoMatchingAssociation()
		{
			var manager = GetManager();
			_externalUserAssociationRepo.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).Returns((ExternalUserAssociation) null);

			var result = manager.ExternalUserAssociationCheck(new ExternalAuthenticationResult(), "");

			Assert.False(result.Successful);
			Assert.Null(result.ExternalUserAssociation);
			Assert.Null(result.User);
		}

		[Fact]
		public void ExternalUserAssociationCheckResultFalseWithNullsIfNoMatchingUser()
		{
			var manager = GetManager();
			_externalUserAssociationRepo.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(new ExternalUserAssociation());
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).Returns((User) null);

			var result = manager.ExternalUserAssociationCheck(new ExternalAuthenticationResult(), "");

			Assert.False(result.Successful);
			Assert.Null(result.ExternalUserAssociation);
			Assert.Null(result.User);
		}

		[Fact]
		public void ExternalUserAssociationCheckResultTrueWithHydratedResultIfMatchingAssociationAndUser()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation { Issuer = "Google", UserID = 123, ProviderKey = "abc"};
			var user = new User {UserID = association.UserID};
			_externalUserAssociationRepo.Setup(x => x.Get(association.Issuer, association.ProviderKey)).Returns(association);
			_userRepo.Setup(x => x.GetUser(association.UserID)).Returns(user);
			var authResult = new ExternalAuthenticationResult {Issuer = "Google", ProviderKey = "abc"};

			var result = manager.ExternalUserAssociationCheck(authResult, "");

			Assert.True(result.Successful);
			Assert.Same(user, result.User);
			Assert.Same(association, result.ExternalUserAssociation);
		}

		[Fact]
		public void ExternalUserAssociationCheckResultTrueCallsSecurityLog()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation { Issuer = "Google", UserID = 123, ProviderKey = "abc" };
			var user = new User {UserID = association.UserID};
			_externalUserAssociationRepo.Setup(x => x.Get(association.Issuer, association.ProviderKey)).Returns(association);
			_userRepo.Setup(x => x.GetUser(association.UserID)).Returns(user);
			const string ip = "1.1.1.1";
			var authResult = new ExternalAuthenticationResult { Issuer = "Google", ProviderKey = "abc" };

			manager.ExternalUserAssociationCheck(authResult, ip);

			_securityLogService.Verify(x => x.CreateLogEntry(user, user, ip, It.IsAny<string>(), SecurityLogType.ExternalAssociationCheckSuccessful));
		}

		[Fact]
		public void ExternalUserAssociationCheckResultFalseNoMatchCallsSecurityLog()
		{
			var manager = GetManager();
			_externalUserAssociationRepo.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).Returns((ExternalUserAssociation)null);
			const string ip = "1.1.1.1";
			var authResult = new ExternalAuthenticationResult { Issuer = "Google", ProviderKey = "abc" };

			manager.ExternalUserAssociationCheck(authResult, ip);

			_securityLogService.Verify(x => x.CreateLogEntry((int?)null, null, ip, It.IsAny<string>(), SecurityLogType.ExternalAssociationCheckFailed), Times.Once());
		}

		[Fact]
		public void ExternalUserAssociationCheckResultFalseNoUserCallsSecurityLog()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation { Issuer = "Google", UserID = 123, ProviderKey = "abc" };
			_externalUserAssociationRepo.Setup(x => x.Get(association.Issuer, association.ProviderKey)).Returns(association);
			_userRepo.Setup(x => x.GetUser(association.UserID)).Returns((User)null);
			const string ip = "1.1.1.1";
			var authResult = new ExternalAuthenticationResult { Issuer = "Google", ProviderKey = "abc" };

			manager.ExternalUserAssociationCheck(authResult, ip);

			_securityLogService.Verify(x => x.CreateLogEntry((int?)null, null, ip, It.IsAny<string>(), SecurityLogType.ExternalAssociationCheckFailed), Times.Once());
		}

		[Fact]
		public void AssociateThrowsWithNullUser()
		{
			var manager = GetManager();

			Assert.Throws<ArgumentNullException>(() => manager.Associate(null, It.IsAny<ExternalLoginInfo>(), String.Empty));
		}

		[Fact]
		public void AssociateNeverCallsRepoWithNullExternalAuthResult()
		{
			var manager = GetManager();

			manager.Associate(new User(), null, String.Empty);

			_externalUserAssociationRepo.Verify(x => x.Save(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
		}

		[Fact]
		public void AssociateThrowsWithNoProviderKey()
		{
			var manager = GetManager();

			Assert.Throws<NullReferenceException>(() => manager.Associate(new User(), new ExternalLoginInfo(new ClaimsPrincipal(), "wegggw", null, "wewg"), String.Empty));
		}

		[Fact]
		public void AssociateMapsObjectsToRepoCall()
		{
			var manager = GetManager();
			var user = new User {UserID = 123};
			var externalAuthResult = new ExternalLoginInfo(new ClaimsPrincipal(), "wegggw", "wfweg", "wewg");

			manager.Associate(user, externalAuthResult, String.Empty);

			_externalUserAssociationRepo.Verify(x => x.Save(user.UserID, externalAuthResult.LoginProvider, externalAuthResult.ProviderKey, externalAuthResult.ProviderDisplayName), Times.Once());
		}

		[Fact]
		public void AssociateSuccessCallsSecurityLog()
		{
			var manager = GetManager();
			var user = new User { UserID = 123 };
			var externalAuthResult = new ExternalLoginInfo(new ClaimsPrincipal(), "wegggw", "wfweg", "wewg");
			const string ip = "1.1.1.1";

			manager.Associate(user, externalAuthResult, ip);

			_securityLogService.Verify(x => x.CreateLogEntry(user, user, ip, It.IsAny<string>(), SecurityLogType.ExternalAssociationSet), Times.Once());
		}

		[Fact]
		public void GetExternalUserAssociationsCallsRepoByUserID()
		{
			var manager = GetManager();
			var user = new User { UserID = 123 };

			manager.GetExternalUserAssociations(user);

			_externalUserAssociationRepo.Verify(x => x.GetByUser(user.UserID), Times.Once());
		}

		[Fact]
		public void GetExternalUserAssociationsReturnsCollectionFromRepo()
		{
			var manager = GetManager();
			var user = new User { UserID = 123 };
			var collection = new List<ExternalUserAssociation>();
			_externalUserAssociationRepo.Setup(x => x.GetByUser(user.UserID)).Returns(collection);

			var result = manager.GetExternalUserAssociations(user);

			Assert.Same(collection, result);
		}

		[Fact]
		public void RemoveAssociationNeverCallsRepoIfNoAssociationIsFound()
		{
			var manager = GetManager();
			_externalUserAssociationRepo.Setup(x => x.Get(It.IsAny<int>())).Returns((ExternalUserAssociation) null);

			manager.RemoveAssociation(new User(), 4556, String.Empty);

			_externalUserAssociationRepo.Verify(x => x.Delete(It.IsAny<int>()), Times.Never());
		}

		[Fact]
		public void RemoveAssociationLogsTheRemoval()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation {ExternalUserAssociationID = 123, Issuer = "Google", Name = "Jeffy", ProviderKey = "oihfoihfef", UserID = 456};
			var user = new User {UserID = association.UserID};
			const string ip = "1.1.1.1";
			_externalUserAssociationRepo.Setup(x => x.Get(association.ExternalUserAssociationID)).Returns(association);

			manager.RemoveAssociation(user, association.ExternalUserAssociationID, ip);

			_securityLogService.Verify(x => x.CreateLogEntry(user, user, ip, It.IsAny<string>(), SecurityLogType.ExternalAssociationRemoved), Times.Once());
		}

		[Fact]
		public void RemoveAssociationThrowsIfUserIDsDontMatch()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation { ExternalUserAssociationID = 123, UserID = 456 };
			var user = new User { UserID = 789 };
			_externalUserAssociationRepo.Setup(x => x.Get(association.ExternalUserAssociationID)).Returns(association);

			Assert.Throws<Exception>(() => manager.RemoveAssociation(user, association.ExternalUserAssociationID, String.Empty));
		}

		[Fact]
		public void RemoveAssociationCallsRepoOnSuccessfulMatch()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation { ExternalUserAssociationID = 123, UserID = 456 };
			var user = new User { UserID = association.UserID };
			_externalUserAssociationRepo.Setup(x => x.Get(association.ExternalUserAssociationID)).Returns(association);

			manager.RemoveAssociation(user, association.ExternalUserAssociationID, String.Empty);

			_externalUserAssociationRepo.Verify(x => x.Delete(association.ExternalUserAssociationID), Times.Once());
		}

		[Fact]
		public void GetExternalUserAssociationsThrowsIfAssociationDoesntMatchUser()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation { ExternalUserAssociationID = 456, UserID = 789};
			var user = new User();
			_externalUserAssociationRepo.Setup(x => x.Get(association.ExternalUserAssociationID)).Returns(association);

			Assert.Throws<Exception>(() => manager.RemoveAssociation(user, association.ExternalUserAssociationID, String.Empty));
		}

		[Fact]
		public void GetExternalUserAssociationsCallsRepoWithMatchingUserIDs()
		{
			var manager = GetManager();
			var user = new User { UserID = 123 };
			var association = new ExternalUserAssociation { ExternalUserAssociationID = 456, UserID = user.UserID };
			_externalUserAssociationRepo.Setup(x => x.Get(association.ExternalUserAssociationID)).Returns(association);

			manager.RemoveAssociation(user, association.ExternalUserAssociationID, String.Empty);

			_externalUserAssociationRepo.Verify(x => x.Delete(association.ExternalUserAssociationID), Times.Once());
		}
	}
}