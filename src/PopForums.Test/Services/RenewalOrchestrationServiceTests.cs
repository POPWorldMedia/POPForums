using PopForums.Models.Subscriptions;
using PopForums.Services.Subscriptions;

namespace PopForums.Test.Services;

public class RenewalOrchestrationServiceTests
{
	private const string TenantID = "tenant1";

	private IRenewalService _renewalService;
	private IRenewalQueueRepository _renewalQueueRepository;
	private ITenantService _tenantService;
	private IErrorLog _errorLog;

	private RenewalOrchestrationService GetService()
	{
		_renewalService = Substitute.For<IRenewalService>();
		_renewalQueueRepository = Substitute.For<IRenewalQueueRepository>();
		_tenantService = Substitute.For<ITenantService>();
		_errorLog = Substitute.For<IErrorLog>();
		_tenantService.GetTenant().Returns(TenantID);
		return new RenewalOrchestrationService(_renewalService, _renewalQueueRepository, _tenantService, _errorLog);
	}

	public class EnqueueTenantsForRenewalTests : RenewalOrchestrationServiceTests
	{
		[Fact]
		public async Task EnqueuesPayloadForEachUserIDWithTenantID()
		{
			var service = GetService();
			var userIDs = new List<int> { 1, 2, 3 };
			_renewalService.GetUserIDsForRenewal().Returns(Task.FromResult<IEnumerable<int>>(userIDs));

			await service.EnqueueTenantsForRenewal();

			await _renewalQueueRepository.Received().Enqueue(Arg.Is<RenewalQueuePayload>(x => x.UserID == 1 && x.TenantID == TenantID));
			await _renewalQueueRepository.Received().Enqueue(Arg.Is<RenewalQueuePayload>(x => x.UserID == 2 && x.TenantID == TenantID));
			await _renewalQueueRepository.Received().Enqueue(Arg.Is<RenewalQueuePayload>(x => x.UserID == 3 && x.TenantID == TenantID));
		}

		[Fact]
		public async Task DoesNothingWhenNoUserIDs()
		{
			var service = GetService();
			_renewalService.GetUserIDsForRenewal().Returns(Task.FromResult<IEnumerable<int>>(new List<int>()));

			await service.EnqueueTenantsForRenewal();

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
			_renewalService.ChargeAndRecordRenewal(UserID).Returns(Task.FromResult(BasicServiceResponse<Transaction>.Failed("card declined")));

			await service.ProcessRenewal(UserID);

			_errorLog.Received().Log(null, ErrorSeverity.Information, Arg.Is<string>(x => x.Contains("card declined") && x.Contains(UserID.ToString())));
		}

		[Fact]
		public async Task DoesNotLogWhenChargeSucceeds()
		{
			var service = GetService();
			var transaction = new Transaction { UserID = UserID };
			_renewalService.ChargeAndRecordRenewal(UserID).Returns(Task.FromResult(BasicServiceResponse<Transaction>.Success(transaction)));

			await service.ProcessRenewal(UserID);

			_errorLog.DidNotReceive().Log(Arg.Any<Exception>(), Arg.Any<ErrorSeverity>(), Arg.Any<string>());
		}
	}
}
