using PopForums.Models.Subscriptions;
using PopForums.Repositories.Subscriptions;
using PopForums.Services.Subscriptions;

namespace PopForums.Test.Services;

public class BuyServiceTests
{
	private const int UserID = 42;
	private const string SkuID = "sku1";

	private ISkuRepository _skuRepository;
	private IUserRepository _userRepository;
	private IBankChargeRepository _bankChargeRepository;
	private ITransactionRepository _transactionRepository;
	private IProfileRepository _profileRepository;
	private ISubscriptionHistoryRepository _subscriptionHistoryRepository;

	private BuyService GetService()
	{
		_skuRepository = Substitute.For<ISkuRepository>();
		_userRepository = Substitute.For<IUserRepository>();
		_bankChargeRepository = Substitute.For<IBankChargeRepository>();
		_transactionRepository = Substitute.For<ITransactionRepository>();
		_profileRepository = Substitute.For<IProfileRepository>();
		_subscriptionHistoryRepository = Substitute.For<ISubscriptionHistoryRepository>();
		return new BuyService(_skuRepository, _userRepository, _bankChargeRepository, _transactionRepository, _profileRepository, _subscriptionHistoryRepository);
	}

	private static Sku GetActiveSku(int months = 1)
	{
		return new Sku { SkuID = SkuID, Name = "Gold Plan", Price = 9.99m, IsActive = true, Months = (ushort)months };
	}

	private static User GetUser(DateOnly? expiration = null)
	{
		return new User { UserID = UserID, Email = "a@b.com", SubscriptionExpiration = expiration };
	}

	public class BuyNewTests : BuyServiceTests
	{
		[Fact]
		public async Task ReturnsFailureWhenSkuIsNull()
		{
			var service = GetService();
			_skuRepository.Get(SkuID).Returns(Task.FromResult<Sku>(null));
			var buyModel = new BuyModel { SkuID = SkuID, Token = "tok" };

			var result = await service.BuyNew(buyModel, UserID);

			Assert.False(result.IsSuccessful);
			Assert.Equal(" That plan is no longer available.", result.Message);
			await _userRepository.DidNotReceive().GetUser(Arg.Any<int>());
			await _bankChargeRepository.DidNotReceive().CreateCustomer(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
		}

		[Fact]
		public async Task ReturnsFailureWhenSkuIsNotActive()
		{
			var service = GetService();
			var sku = GetActiveSku();
			sku.IsActive = false;
			_skuRepository.Get(SkuID).Returns(Task.FromResult(sku));
			var buyModel = new BuyModel { SkuID = SkuID, Token = "tok" };

			var result = await service.BuyNew(buyModel, UserID);

			Assert.False(result.IsSuccessful);
			Assert.Equal(" That plan is no longer available.", result.Message);
		}

		[Fact]
		public async Task ReturnsFailureWhenCreateCustomerFails()
		{
			var service = GetService();
			var sku = GetActiveSku();
			_skuRepository.Get(SkuID).Returns(Task.FromResult(sku));
			var user = GetUser();
			_userRepository.GetUser(UserID).Returns(Task.FromResult(user));
			_bankChargeRepository.CreateCustomer(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
				.Returns(Task.FromResult(BasicServiceResponse<CreateCustomerResult>.Failed("card declined")));
			var buyModel = new BuyModel { SkuID = SkuID, Token = "tok" };

			var result = await service.BuyNew(buyModel, UserID);

			Assert.False(result.IsSuccessful);
			Assert.Equal("card declined", result.Message);
			await _bankChargeRepository.DidNotReceive().ChargeCustomer(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
		}

		[Fact]
		public async Task ReturnsFailureWhenChargeCustomerFails()
		{
			var service = GetService();
			var sku = GetActiveSku();
			_skuRepository.Get(SkuID).Returns(Task.FromResult(sku));
			var user = GetUser();
			_userRepository.GetUser(UserID).Returns(Task.FromResult(user));
			_bankChargeRepository.CreateCustomer(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
				.Returns(Task.FromResult(BasicServiceResponse<CreateCustomerResult>.Success(new CreateCustomerResult { CustomerID = "cust1", Last4 = "1234" })));
			_bankChargeRepository.ChargeCustomer(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(BasicServiceResponse<Transaction>.Failed("charge failed")));
			var buyModel = new BuyModel { SkuID = SkuID, Token = "tok" };

			var result = await service.BuyNew(buyModel, UserID);

			Assert.False(result.IsSuccessful);
			Assert.Equal("charge failed", result.Message);
			await _transactionRepository.DidNotReceive().Create(Arg.Any<Transaction>());
			await _profileRepository.DidNotReceive().Update(Arg.Any<Profile>());
		}

		[Fact]
		public async Task SucceedsAndUpdatesEverythingWhenNoExistingExpiration()
		{
			var service = GetService();
			var sku = GetActiveSku(3);
			_skuRepository.Get(SkuID).Returns(Task.FromResult(sku));
			var user = GetUser();
			_userRepository.GetUser(UserID).Returns(Task.FromResult(user));
			_bankChargeRepository.CreateCustomer(Arg.Any<string>(), user.Email, UserID)
				.Returns(Task.FromResult(BasicServiceResponse<CreateCustomerResult>.Success(new CreateCustomerResult { CustomerID = "cust1", Last4 = "4242" })));
			var transaction = new Transaction { CustomerID = "cust1", Last4 = "4242", UserID = UserID, SkuID = SkuID, Amount = sku.Price };
			_bankChargeRepository.ChargeCustomer("cust1", UserID, sku.Price, Arg.Any<DateTime>(), SkuID, sku.Name, user.Email)
				.Returns(Task.FromResult(BasicServiceResponse<Transaction>.Success(transaction)));
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(new Profile { UserID = UserID }));
			Profile updatedProfile = null;
			await _profileRepository.Update(Arg.Do<Profile>(x => updatedProfile = x));
			SubscriptionHistory history = null;
			await _subscriptionHistoryRepository.Create(Arg.Do<SubscriptionHistory>(x => history = x));
			var buyModel = new BuyModel { SkuID = SkuID, Token = "tok" };

			var result = await service.BuyNew(buyModel, UserID);

			Assert.True(result.IsSuccessful);
			Assert.Equal(transaction, result.Data);
			await _transactionRepository.Received().Create(transaction);

			await _profileRepository.Received().Update(Arg.Any<Profile>());
			Assert.Equal("4242", updatedProfile.Last4);
			Assert.Equal("cust1", updatedProfile.CustomerID);
			Assert.Equal(SkuID, updatedProfile.SkuID);

			var expectedExpiration = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(3);
			await _userRepository.Received().UpdateSubscriptionExpiration(UserID, expectedExpiration);

			await _subscriptionHistoryRepository.Received().Create(Arg.Any<SubscriptionHistory>());
			Assert.Equal(UserID, history.UserID);
			Assert.Equal(SkuID, history.SkuID);
			Assert.Contains("4242", history.Message);
			Assert.Contains(sku.Name, history.Message);
		}

		[Fact]
		public async Task ExtendsExistingFutureExpiration()
		{
			var service = GetService();
			var sku = GetActiveSku(2);
			_skuRepository.Get(SkuID).Returns(Task.FromResult(sku));
			var futureExpiration = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(1);
			var user = GetUser(futureExpiration);
			_userRepository.GetUser(UserID).Returns(Task.FromResult(user));
			_bankChargeRepository.CreateCustomer(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
				.Returns(Task.FromResult(BasicServiceResponse<CreateCustomerResult>.Success(new CreateCustomerResult { CustomerID = "cust1", Last4 = "4242" })));
			var transaction = new Transaction { CustomerID = "cust1", Last4 = "4242", UserID = UserID, SkuID = SkuID, Amount = sku.Price };
			_bankChargeRepository.ChargeCustomer(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(BasicServiceResponse<Transaction>.Success(transaction)));
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(new Profile { UserID = UserID }));
			var buyModel = new BuyModel { SkuID = SkuID, Token = "tok" };

			await service.BuyNew(buyModel, UserID);

			var expectedExpiration = futureExpiration.AddMonths(2);
			await _userRepository.Received().UpdateSubscriptionExpiration(UserID, expectedExpiration);
		}

		[Fact]
		public async Task ResetsExpirationFromTodayWhenExistingExpirationIsPast()
		{
			var service = GetService();
			var sku = GetActiveSku(1);
			_skuRepository.Get(SkuID).Returns(Task.FromResult(sku));
			var pastExpiration = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-1);
			var user = GetUser(pastExpiration);
			_userRepository.GetUser(UserID).Returns(Task.FromResult(user));
			_bankChargeRepository.CreateCustomer(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
				.Returns(Task.FromResult(BasicServiceResponse<CreateCustomerResult>.Success(new CreateCustomerResult { CustomerID = "cust1", Last4 = "4242" })));
			var transaction = new Transaction { CustomerID = "cust1", Last4 = "4242", UserID = UserID, SkuID = SkuID, Amount = sku.Price };
			_bankChargeRepository.ChargeCustomer(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
				.Returns(Task.FromResult(BasicServiceResponse<Transaction>.Success(transaction)));
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(new Profile { UserID = UserID }));
			var buyModel = new BuyModel { SkuID = SkuID, Token = "tok" };

			await service.BuyNew(buyModel, UserID);

			var expectedExpiration = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(1);
			await _userRepository.Received().UpdateSubscriptionExpiration(UserID, expectedExpiration);
		}
	}

	public class UpdatePaymentMethodTests : BuyServiceTests
	{
		[Fact]
		public async Task ReturnsFailureWhenCreateCustomerFails()
		{
			var service = GetService();
			var user = GetUser();
			_userRepository.GetUser(UserID).Returns(Task.FromResult(user));
			_bankChargeRepository.CreateCustomer(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
				.Returns(Task.FromResult(BasicServiceResponse<CreateCustomerResult>.Failed("card declined")));

			var result = await service.UpdatePaymentMethod(UserID, "tok");

			Assert.False(result.IsSuccessful);
			Assert.Equal("card declined", result.Message);
			await _profileRepository.DidNotReceive().GetProfile(Arg.Any<int>());
			await _subscriptionHistoryRepository.DidNotReceive().Create(Arg.Any<SubscriptionHistory>());
		}

		[Fact]
		public async Task Succeeds()
		{
			var service = GetService();
			var user = GetUser();
			_userRepository.GetUser(UserID).Returns(Task.FromResult(user));
			_bankChargeRepository.CreateCustomer("newtoken", user.Email, UserID)
				.Returns(Task.FromResult(BasicServiceResponse<CreateCustomerResult>.Success(new CreateCustomerResult { CustomerID = "newCust", Last4 = "9999" })));
			var profile = new Profile { UserID = UserID, SkuID = "existingSku", Last4 = "1111", CustomerID = "oldCust" };
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(profile));
			Profile updatedProfile = null;
			await _profileRepository.Update(Arg.Do<Profile>(x => updatedProfile = x));
			SubscriptionHistory history = null;
			await _subscriptionHistoryRepository.Create(Arg.Do<SubscriptionHistory>(x => history = x));

			var result = await service.UpdatePaymentMethod(UserID, "newtoken");

			Assert.True(result.IsSuccessful);
			Assert.Equal("9999", result.Data);

			await _profileRepository.Received().Update(Arg.Any<Profile>());
			Assert.Equal("9999", updatedProfile.Last4);
			Assert.Equal("newCust", updatedProfile.CustomerID);

			await _subscriptionHistoryRepository.Received().Create(Arg.Any<SubscriptionHistory>());
			Assert.Equal(UserID, history.UserID);
			Assert.Equal("existingSku", history.SkuID);
			Assert.Equal("New card: 9999", history.Message);
		}
	}
}
