using PopForums.Models.Subscriptions;
using PopForums.Services.Subscriptions;

namespace PopForums.Test.Services;

public class RenewalOrchestrationServiceTests
{
	private const string TenantID = "tenant1";
	private const string SkuID = "sku1";
	private const string SkuName = "Gold Plan";

	private IRenewalService _renewalService;
	private IRenewalQueueRepository _renewalQueueRepository;
	private ITenantService _tenantService;
	private IErrorLog _errorLog;
	private ISettingsManager _settingsManager;
	private INotificationTunnel _notificationTunnel;
	private IProfileRepository _profileRepository;
	private ISkuService _skuService;

	private RenewalOrchestrationService GetService()
	{
		_renewalService = Substitute.For<IRenewalService>();
		_renewalQueueRepository = Substitute.For<IRenewalQueueRepository>();
		_tenantService = Substitute.For<ITenantService>();
		_errorLog = Substitute.For<IErrorLog>();
		_settingsManager = Substitute.For<ISettingsManager>();
		_settingsManager.Current.Returns(new Settings { IsSubscriptionEnabled = true });
		_notificationTunnel = Substitute.For<INotificationTunnel>();
		_profileRepository = Substitute.For<IProfileRepository>();
		_skuService = Substitute.For<ISkuService>();
		_tenantService.GetTenant().Returns(TenantID);
		return new RenewalOrchestrationService(_renewalService, _renewalQueueRepository, _tenantService, _errorLog, _settingsManager, _notificationTunnel, _profileRepository, _skuService);
	}

	public class EnqueueUsersForRenewalTests : RenewalOrchestrationServiceTests
	{
		[Fact]
		public async Task EnqueuesPayloadForEachUserIDWithTenantID()
		{
			var service = GetService();
			var userIDs = new List<int> { 1, 2, 3 };
			_renewalService.GetUserIDsForRenewal().Returns(Task.FromResult<IEnumerable<int>>(userIDs));

			await service.EnqueueUsersForRenewal();

			await _renewalQueueRepository.Received().Enqueue(Arg.Is<RenewalQueuePayload>(x => x.UserID == 1 && x.TenantID == TenantID));
			await _renewalQueueRepository.Received().Enqueue(Arg.Is<RenewalQueuePayload>(x => x.UserID == 2 && x.TenantID == TenantID));
			await _renewalQueueRepository.Received().Enqueue(Arg.Is<RenewalQueuePayload>(x => x.UserID == 3 && x.TenantID == TenantID));
		}

		[Fact]
		public async Task DoesNothingWhenNoUserIDs()
		{
			var service = GetService();
			_renewalService.GetUserIDsForRenewal().Returns(Task.FromResult<IEnumerable<int>>(new List<int>()));

			await service.EnqueueUsersForRenewal();

			await _renewalQueueRepository.DidNotReceive().Enqueue(Arg.Any<RenewalQueuePayload>());
		}

		[Fact]
		public async Task DoesNothingWhenSubscriptionsAreNotEnabled()
		{
			var service = GetService();
			_settingsManager.Current.Returns(new Settings { IsSubscriptionEnabled = false });

			await service.EnqueueUsersForRenewal();

			await _renewalService.DidNotReceive().GetUserIDsForRenewal();
			await _renewalQueueRepository.DidNotReceive().Enqueue(Arg.Any<RenewalQueuePayload>());
		}
	}

	public class ProcessRenewalTests : RenewalOrchestrationServiceTests
	{
		private const int UserID = 42;

		[Fact]
		public async Task LogsInformationWhenChargeFails()
		{
			var service = GetService();
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(new Profile { UserID = UserID, SkuID = SkuID }));
			_skuService.Get(SkuID).Returns(Task.FromResult(new Sku { SkuID = SkuID, Name = SkuName }));
			_renewalService.ChargeAndRecordRenewal(UserID).Returns(Task.FromResult(BasicServiceResponse<Transaction>.Failed("card declined")));

			await service.ProcessRenewal(UserID);

			_errorLog.Received().Log(null, ErrorSeverity.Information, Arg.Is<string>(x => x.Contains("card declined") && x.Contains(UserID.ToString())));
			_notificationTunnel.Received().SendNotificationForSubscriptionRenewalFailed(UserID, SkuName, TenantID);
			_notificationTunnel.DidNotReceive().SendNotificationForSubscriptionRenewed(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
		}

		[Fact]
		public async Task DoesNotLogWhenChargeSucceeds()
		{
			var service = GetService();
			_profileRepository.GetProfile(UserID).Returns(Task.FromResult(new Profile { UserID = UserID, SkuID = SkuID }));
			_skuService.Get(SkuID).Returns(Task.FromResult(new Sku { SkuID = SkuID, Name = SkuName }));
			var transaction = new Transaction { UserID = UserID };
			_renewalService.ChargeAndRecordRenewal(UserID).Returns(Task.FromResult(BasicServiceResponse<Transaction>.Success(transaction)));

			await service.ProcessRenewal(UserID);

			_errorLog.DidNotReceive().Log(Arg.Any<Exception>(), Arg.Any<ErrorSeverity>(), Arg.Any<string>());
			_notificationTunnel.Received().SendNotificationForSubscriptionRenewed(UserID, SkuName, TenantID);
			_notificationTunnel.DidNotReceive().SendNotificationForSubscriptionRenewalFailed(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
		}

		[Fact]
		public async Task DoesNothingWhenSubscriptionsAreNotEnabled()
		{
			var service = GetService();
			_settingsManager.Current.Returns(new Settings { IsSubscriptionEnabled = false });

			await service.ProcessRenewal(UserID);

			await _renewalService.DidNotReceive().ChargeAndRecordRenewal(Arg.Any<int>());
		}
	}
}
