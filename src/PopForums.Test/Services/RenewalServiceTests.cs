using PopForums.Models.Subscriptions;
using PopForums.Repositories.Subscriptions;
using PopForums.Services.Subscriptions;

namespace PopForums.Test.Services;

public class RenewalServiceTests
{
	private const int UserID = 42;
	private const string SkuID = "sku1";

	private IUserRepository _userRepository;
	private ISkuRepository _skuRepository;
	private IBankChargeRepository _bankChargeRepository;
	private ITransactionRepository _transactionRepository;
	private IProfileRepository _profileRepository;
	private ISubscriptionHistoryRepository _subscriptionHistoryRepository;
	private ISettingsManager _settingsManager;

	private RenewalService GetService()
	{
		_userRepository = Substitute.For<IUserRepository>();
		_skuRepository = Substitute.For<ISkuRepository>();
		_bankChargeRepository = Substitute.For<IBankChargeRepository>();
		_transactionRepository = Substitute.For<ITransactionRepository>();
		_profileRepository = Substitute.For<IProfileRepository>();
		_subscriptionHistoryRepository = Substitute.For<ISubscriptionHistoryRepository>();
		_settingsManager = Substitute.For<ISettingsManager>();
		_settingsManager.Current.Returns(new Settings());
		return new RenewalService(_userRepository, _skuRepository, _bankChargeRepository, _transactionRepository, _profileRepository, _subscriptionHistoryRepository, _settingsManager);
	}

	private static Sku GetSku(int months = 1, bool isActive = true)
	{
		return new Sku { SkuID = SkuID, Name = "Gold Plan", Price = 9.99m, IsActive = isActive, Months = (ushort)months };
	}

	private static User GetUser(DateOnly? expiration = null)
	{
		return new User { UserID = UserID, Email = "a@b.com", SubscriptionExpiration = expiration };
	}

	private static Profile GetProfile()
	{
		return new Profile { UserID = UserID, SkuID = SkuID, CustomerID = "cust1", Last4 = "4242" };
	}

	public class GetUserIDsForRenewalTests : RenewalServiceTests
	{
		[Fact]
		public async Task ReturnsUserIDsWithExpirationOfToday()
		{
			var service = GetService();
			var today = DateOnly.FromDateTime(DateTime.UtcNow);
			var ids = new List<int> { 1, 2, 3 };
			_userRepository.GetUserIDsBySubscriptionExpiration(today).Returns(Task.FromResult(ids));

			var result = await service.GetUserIDsForRenewal();

			Assert.Equal(ids, result);
			await _userRepository.Received().GetUserIDsBySubscriptionExpiration(today);
		}
	}

	public class ChargeAndRecordRenewalTests : RenewalServiceTests
	{
		[Fact]
		public async Task ThrowsWhenSkuIsNull()
		{
			var service = GetService();
			_userRepository.GetUser(UserID).Returns(Task.FromResult(GetUser()));
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(GetProfile()));
			_skuRepository.Get(SkuID).Returns(Task.FromResult<Sku>(null));

			await Assert.ThrowsAsync<Exception>(() => service.ChargeAndRecordRenewal(UserID));

			await _bankChargeRepository.DidNotReceive().ChargeCustomer(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
		}

		[Fact]
		public async Task UsesInactiveSkuWithoutThrowing()
		{
			var service = GetService();
			var user = GetUser();
			_userRepository.GetUser(UserID).Returns(Task.FromResult(user));
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(GetProfile()));
			var sku = GetSku(isActive: false);
			_skuRepository.Get(SkuID).Returns(Task.FromResult(sku));
			var transaction = new Transaction { CustomerID = "cust1", Last4 = "4242", UserID = UserID, SkuID = SkuID, Amount = sku.Price };
			_bankChargeRepository.ChargeCustomer(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(BasicServiceResponse<Transaction>.Success(transaction)));

			var result = await service.ChargeAndRecordRenewal(UserID);

			Assert.True(result.IsSuccessful);
		}

		[Fact]
		public async Task ReturnsFailureWhenChargeCustomerFails()
		{
			var service = GetService();
			_userRepository.GetUser(UserID).Returns(Task.FromResult(GetUser()));
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(GetProfile()));
			_skuRepository.Get(SkuID).Returns(Task.FromResult(GetSku()));
			_bankChargeRepository.ChargeCustomer(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(BasicServiceResponse<Transaction>.Failed("charge failed")));

			var result = await service.ChargeAndRecordRenewal(UserID);

			Assert.False(result.IsSuccessful);
			Assert.Equal("charge failed", result.Message);
			await _transactionRepository.DidNotReceive().Create(Arg.Any<Transaction>());
			await _userRepository.DidNotReceive().UpdateSubscriptionExpiration(Arg.Any<int>(), Arg.Any<DateOnly?>());
		}

		[Fact]
		public async Task DoesNotCreateACustomer()
		{
			var service = GetService();
			_userRepository.GetUser(UserID).Returns(Task.FromResult(GetUser()));
			var profile = GetProfile();
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(profile));
			var sku = GetSku();
			_skuRepository.Get(SkuID).Returns(Task.FromResult(sku));
			var transaction = new Transaction { CustomerID = profile.CustomerID, Last4 = profile.Last4, UserID = UserID, SkuID = SkuID, Amount = sku.Price };
			_bankChargeRepository.ChargeCustomer(profile.CustomerID, UserID, sku.Price, Arg.Any<DateTime>(), SkuID, sku.Name, "a@b.com")
				.Returns(Task.FromResult(BasicServiceResponse<Transaction>.Success(transaction)));

			await service.ChargeAndRecordRenewal(UserID);

			await _bankChargeRepository.DidNotReceive().CreateCustomer(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
		}

		[Fact]
		public async Task SucceedsAndDoesNotUpdateProfile()
		{
			var service = GetService();
			_userRepository.GetUser(UserID).Returns(Task.FromResult(GetUser()));
			var profile = GetProfile();
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(profile));
			var sku = GetSku();
			_skuRepository.Get(SkuID).Returns(Task.FromResult(sku));
			var transaction = new Transaction { CustomerID = profile.CustomerID, Last4 = profile.Last4, UserID = UserID, SkuID = SkuID, Amount = sku.Price };
			_bankChargeRepository.ChargeCustomer(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(BasicServiceResponse<Transaction>.Success(transaction)));

			var result = await service.ChargeAndRecordRenewal(UserID);

			Assert.True(result.IsSuccessful);
			Assert.Equal(transaction, result.Data);
			await _transactionRepository.Received().Create(transaction);
			await _profileRepository.DidNotReceive().Update(Arg.Any<Profile>());
		}

		[Fact]
		public async Task ExtendsFromExistingExpiration()
		{
			var service = GetService();
			var expiration = DateOnly.FromDateTime(DateTime.UtcNow);
			_userRepository.GetUser(UserID).Returns(Task.FromResult(GetUser(expiration)));
			var profile = GetProfile();
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(profile));
			var sku = GetSku(3);
			_skuRepository.Get(SkuID).Returns(Task.FromResult(sku));
			var transaction = new Transaction { CustomerID = profile.CustomerID, Last4 = profile.Last4, UserID = UserID, SkuID = SkuID, Amount = sku.Price };
			_bankChargeRepository.ChargeCustomer(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(BasicServiceResponse<Transaction>.Success(transaction)));

			await service.ChargeAndRecordRenewal(UserID);

			var expectedExpiration = expiration.AddMonths(3);
			await _userRepository.Received().UpdateSubscriptionExpiration(UserID, expectedExpiration);
		}

		[Fact]
		public async Task UsesTodayWhenNoExistingExpiration()
		{
			var service = GetService();
			_userRepository.GetUser(UserID).Returns(Task.FromResult(GetUser()));
			var profile = GetProfile();
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(profile));
			var sku = GetSku(2);
			_skuRepository.Get(SkuID).Returns(Task.FromResult(sku));
			var transaction = new Transaction { CustomerID = profile.CustomerID, Last4 = profile.Last4, UserID = UserID, SkuID = SkuID, Amount = sku.Price };
			_bankChargeRepository.ChargeCustomer(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(BasicServiceResponse<Transaction>.Success(transaction)));

			await service.ChargeAndRecordRenewal(UserID);

			var expectedExpiration = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(2);
			await _userRepository.Received().UpdateSubscriptionExpiration(UserID, expectedExpiration);
		}

		[Fact]
		public async Task RecordsSubscriptionHistory()
		{
			var service = GetService();
			_userRepository.GetUser(UserID).Returns(Task.FromResult(GetUser()));
			var profile = GetProfile();
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(profile));
			var sku = GetSku();
			_skuRepository.Get(SkuID).Returns(Task.FromResult(sku));
			var transaction = new Transaction { CustomerID = profile.CustomerID, Last4 = profile.Last4, UserID = UserID, SkuID = SkuID, Amount = sku.Price };
			_bankChargeRepository.ChargeCustomer(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(BasicServiceResponse<Transaction>.Success(transaction)));
			SubscriptionHistory history = null;
			await _subscriptionHistoryRepository.Create(Arg.Do<SubscriptionHistory>(x => history = x));

			await service.ChargeAndRecordRenewal(UserID);

			await _subscriptionHistoryRepository.Received().Create(Arg.Any<SubscriptionHistory>());
			Assert.Equal(UserID, history.UserID);
			Assert.Equal(SkuID, history.SkuID);
			Assert.Contains("Renewal", history.Message);
			Assert.Contains(profile.Last4, history.Message);
		}
	}
}
