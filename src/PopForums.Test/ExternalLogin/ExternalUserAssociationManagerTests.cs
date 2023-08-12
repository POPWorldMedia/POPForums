namespace PopForums.Test.ExternalLogin;

public class ExternalUserAssociationManagerTests
{
	private ExternalUserAssociationManager GetManager()
	{
		_externalUserAssociationRepo = Substitute.For<IExternalUserAssociationRepository>();
		_userRepo = Substitute.For<IUserRepository>();
		_securityLogService = Substitute.For<ISecurityLogService>();
		return new ExternalUserAssociationManager(_externalUserAssociationRepo, _userRepo, _securityLogService);
	}

	private IExternalUserAssociationRepository _externalUserAssociationRepo;
	private IUserRepository _userRepo;
	private ISecurityLogService _securityLogService;

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
		_externalUserAssociationRepo.Get(Arg.Any<string>(), Arg.Any<string>()).Returns((ExternalUserAssociation) null);

		var result = await manager.ExternalUserAssociationCheck(new ExternalLoginInfo("", "", ""), "");

		Assert.False(result.Successful);
		Assert.Null(result.ExternalUserAssociation);
		Assert.Null(result.User);
	}

	[Fact]
	public async Task ExternalUserAssociationCheckResultFalseWithNullsIfNoMatchingUser()
	{
		var manager = GetManager();
		_externalUserAssociationRepo.Get(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(new ExternalUserAssociation()));
		_userRepo.GetUser(Arg.Any<int>()).Returns((User) null);

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
		_externalUserAssociationRepo.Get(association.Issuer, association.ProviderKey).Returns(Task.FromResult(association));
		_userRepo.GetUser(association.UserID).Returns(Task.FromResult(user));
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
		_externalUserAssociationRepo.Get(association.Issuer, association.ProviderKey).Returns(Task.FromResult(association));
		_userRepo.GetUser(association.UserID).Returns(Task.FromResult(user));
		const string ip = "1.1.1.1";
		var authResult = new ExternalLoginInfo("Google", "abc", "");

		await manager.ExternalUserAssociationCheck(authResult, ip);

		await _securityLogService.Received().CreateLogEntry(user, user, ip, Arg.Any<string>(), SecurityLogType.ExternalAssociationCheckSuccessful);
	}

	[Fact]
	public async Task ExternalUserAssociationCheckResultFalseNoMatchCallsSecurityLog()
	{
		var manager = GetManager();
		_externalUserAssociationRepo.Get(Arg.Any<string>(), Arg.Any<string>()).Returns((ExternalUserAssociation)null);
		const string ip = "1.1.1.1";
		var authResult = new ExternalLoginInfo("Google", "abc", "");

		await manager.ExternalUserAssociationCheck(authResult, ip);

		await _securityLogService.Received().CreateLogEntry((int?)null, null, ip, Arg.Any<string>(), SecurityLogType.ExternalAssociationCheckFailed);
	}

	[Fact]
	public async Task ExternalUserAssociationCheckResultFalseNoUserCallsSecurityLog()
	{
		var manager = GetManager();
		var association = new ExternalUserAssociation { Issuer = "Google", UserID = 123, ProviderKey = "abc" };
		_externalUserAssociationRepo.Get(association.Issuer, association.ProviderKey).Returns(Task.FromResult(association));
		_userRepo.GetUser(association.UserID).Returns((User)null);
		const string ip = "1.1.1.1";
		var authResult = new ExternalLoginInfo("Google", "abc", "");

		await manager.ExternalUserAssociationCheck(authResult, ip);

		await _securityLogService.Received().CreateLogEntry((int?)null, null, ip, Arg.Any<string>(), SecurityLogType.ExternalAssociationCheckFailed);
	}

	[Fact]
	public async Task AssociateThrowsWithNullUser()
	{
		var manager = GetManager();

		await Assert.ThrowsAsync<ArgumentNullException>(async () => await manager.Associate(null, default, String.Empty));
	}

	[Fact]
	public async Task AssociateNeverCallsRepoWithNullExternalAuthResult()
	{
		var manager = GetManager();

		await manager.Associate(new User(), null, String.Empty);

		await _externalUserAssociationRepo.DidNotReceive().Save(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
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

		await _externalUserAssociationRepo.Received().Save(user.UserID, externalAuthResult.LoginProvider, externalAuthResult.ProviderKey, externalAuthResult.ProviderDisplayName);
	}

	[Fact]
	public async Task AssociateSuccessCallsSecurityLog()
	{
		var manager = GetManager();
		var user = new User { UserID = 123 };
		var externalAuthResult = new ExternalLoginInfo("wegggw", "wfweg", "wewg");
		const string ip = "1.1.1.1";

		await manager.Associate(user, externalAuthResult, ip);

		await _securityLogService.Received().CreateLogEntry(user, user, ip, Arg.Any<string>(), SecurityLogType.ExternalAssociationSet);
	}

	[Fact]
	public async Task GetExternalUserAssociationsCallsRepoByUserID()
	{
		var manager = GetManager();
		var user = new User { UserID = 123 };
		await _externalUserAssociationRepo.GetByUser(user.UserID);

		await manager.GetExternalUserAssociations(user);

		await _externalUserAssociationRepo.Received().GetByUser(user.UserID);
	}

	[Fact]
	public async Task GetExternalUserAssociationsReturnsCollectionFromRepo()
	{
		var manager = GetManager();
		var user = new User { UserID = 123 };
		var collection = new List<ExternalUserAssociation>();
		_externalUserAssociationRepo.GetByUser(user.UserID).Returns(Task.FromResult(collection));

		var result = await manager.GetExternalUserAssociations(user);

		Assert.Same(collection, result);
	}

	[Fact]
	public async Task RemoveAssociationNeverCallsRepoIfNoAssociationIsFound()
	{
		var manager = GetManager();
		_externalUserAssociationRepo.Get(Arg.Any<int>()).Returns((ExternalUserAssociation) null);

		await manager.RemoveAssociation(new User(), 4556, String.Empty);

		await _externalUserAssociationRepo.DidNotReceive().Delete(Arg.Any<int>());
	}

	[Fact]
	public async Task RemoveAssociationLogsTheRemoval()
	{
		var manager = GetManager();
		var association = new ExternalUserAssociation {ExternalUserAssociationID = 123, Issuer = "Google", Name = "Jeffy", ProviderKey = "oihfoihfef", UserID = 456};
		var user = new User {UserID = association.UserID};
		const string ip = "1.1.1.1";
		_externalUserAssociationRepo.Get(association.ExternalUserAssociationID).Returns(Task.FromResult(association));

		await manager.RemoveAssociation(user, association.ExternalUserAssociationID, ip);

		await _securityLogService.Received().CreateLogEntry(user, user, ip, Arg.Any<string>(), SecurityLogType.ExternalAssociationRemoved);
	}

	[Fact]
	public async Task RemoveAssociationThrowsIfUserIDsDontMatch()
	{
		var manager = GetManager();
		var association = new ExternalUserAssociation { ExternalUserAssociationID = 123, UserID = 456 };
		var user = new User { UserID = 789 };
		_externalUserAssociationRepo.Get(association.ExternalUserAssociationID).Returns(Task.FromResult(association));

		await Assert.ThrowsAsync<Exception>(async () => await manager.RemoveAssociation(user, association.ExternalUserAssociationID, string.Empty));
	}

	[Fact]
	public async Task RemoveAssociationCallsRepoOnSuccessfulMatch()
	{
		var manager = GetManager();
		var association = new ExternalUserAssociation { ExternalUserAssociationID = 123, UserID = 456 };
		var user = new User { UserID = association.UserID };
		_externalUserAssociationRepo.Get(association.ExternalUserAssociationID).Returns(Task.FromResult(association));

		await manager.RemoveAssociation(user, association.ExternalUserAssociationID, String.Empty);

		await _externalUserAssociationRepo.Received().Delete(association.ExternalUserAssociationID);
	}

	[Fact]
	public async Task GetExternalUserAssociationsThrowsIfAssociationDoesntMatchUser()
	{
		var manager = GetManager();
		var association = new ExternalUserAssociation { ExternalUserAssociationID = 456, UserID = 789};
		var user = new User();
		_externalUserAssociationRepo.Get(association.ExternalUserAssociationID).Returns(Task.FromResult(association));

		await Assert.ThrowsAsync<Exception>(async () => await manager.RemoveAssociation(user, association.ExternalUserAssociationID, string.Empty));
	}

	[Fact]
	public async Task GetExternalUserAssociationsCallsRepoWithMatchingUserIDs()
	{
		var manager = GetManager();
		var user = new User { UserID = 123 };
		var association = new ExternalUserAssociation { ExternalUserAssociationID = 456, UserID = user.UserID };
		_externalUserAssociationRepo.Get(association.ExternalUserAssociationID).Returns(Task.FromResult(association));

		await manager.RemoveAssociation(user, association.ExternalUserAssociationID, String.Empty);

		await _externalUserAssociationRepo.Received().Delete(association.ExternalUserAssociationID);
	}
}