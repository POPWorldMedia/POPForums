using System;
using Moq;
using NUnit.Framework;
using PopForums.ExternalLogin;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Test.ExternalLogin
{
	[TestFixture]
	public class UserAssociationManagerTests
	{
		private UserAssociationManager GetManager()
		{
			_externalUserAssociationRepo = new Mock<IExternalUserAssociationRepository>();
			_userRepo = new Mock<IUserRepository>();
			return new UserAssociationManager(_externalUserAssociationRepo.Object, _userRepo.Object);
		}

		private Mock<IExternalUserAssociationRepository> _externalUserAssociationRepo;
		private Mock<IUserRepository> _userRepo;

		[Test]
		public void ExternalUserAssociationCheckThrowsWithNullArg()
		{
			var manager = GetManager();

			Assert.Throws<ArgumentNullException>(() => manager.ExternalUserAssociationCheck(null));
		}

		[Test]
		public void ExternalUserAssociationCheckResultFalseWithNullsIfNoMatchingAssociation()
		{
			var manager = GetManager();
			_externalUserAssociationRepo.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).Returns((ExternalUserAssociation) null);

			var result = manager.ExternalUserAssociationCheck(new ExternalAuthenticationResult());

			Assert.IsFalse(result.Successful);
			Assert.IsNull(result.ExternalUserAssociation);
			Assert.IsNull(result.User);
		}

		[Test]
		public void ExternalUserAssociationCheckResultFalseWithNullsIfNoMatchingUser()
		{
			var manager = GetManager();
			_externalUserAssociationRepo.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(new ExternalUserAssociation());
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).Returns((User) null);

			var result = manager.ExternalUserAssociationCheck(new ExternalAuthenticationResult());

			Assert.IsFalse(result.Successful);
			Assert.IsNull(result.ExternalUserAssociation);
			Assert.IsNull(result.User);
		}

		[Test]
		public void ExternalUserAssociationCheckResultTrueWithHydratedResultIfMatchingAssociationAndUser()
		{
			var manager = GetManager();
			var association = new ExternalUserAssociation { Issuer = "Google", UserID = 123, ProviderKey = "abc"};
			var user = new User(association.UserID, DateTime.MinValue);
			_externalUserAssociationRepo.Setup(x => x.Get(association.Issuer, association.ProviderKey)).Returns(association);
			_userRepo.Setup(x => x.GetUser(association.UserID)).Returns(user);
			var authResult = new ExternalAuthenticationResult {Issuer = "Google", ProviderKey = "abc"};

			var result = manager.ExternalUserAssociationCheck(authResult);

			Assert.IsTrue(result.Successful);
			Assert.AreSame(user, result.User);
			Assert.AreSame(association, result.ExternalUserAssociation);
		}

		[Test]
		public void AssociateThrowsWithNullUser()
		{
			var manager = GetManager();

			Assert.Throws<ArgumentNullException>(() => manager.Associate(null, It.IsAny<ExternalAuthenticationResult>()));
		}

		[Test]
		public void AssociateNeverCallsRepoWithNullExternalAuthResult()
		{
			var manager = GetManager();

			manager.Associate(new User(1, DateTime.MinValue), null);

			_externalUserAssociationRepo.Verify(x => x.Save(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void AssociateThrowsWithNoIssuer()
		{
			var manager = GetManager();

			Assert.Throws<NullReferenceException>(() => manager.Associate(new User(1, DateTime.MinValue), new ExternalAuthenticationResult { Issuer = null, ProviderKey = "aoihfe" }));
		}

		[Test]
		public void AssociateThrowsWithNoProviderKey()
		{
			var manager = GetManager();

			Assert.Throws<NullReferenceException>(() => manager.Associate(new User(1, DateTime.MinValue), new ExternalAuthenticationResult { Issuer = "waoeifhwe", ProviderKey = null }));
		}

		[Test]
		public void AssociateMapsObjectsToRepoCall()
		{
			var manager = GetManager();
			var user = new User(123, DateTime.MinValue);
			var externalAuthResult = new ExternalAuthenticationResult {Issuer = "weihf", ProviderKey = "weoihf", Name = "woehf"};

			manager.Associate(user, externalAuthResult);

			_externalUserAssociationRepo.Verify(x => x.Save(user.UserID, externalAuthResult.Issuer, externalAuthResult.ProviderKey, externalAuthResult.Name), Times.Once());
		}
	}
}