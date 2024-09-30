using NSubstitute.ReturnsExtensions;

namespace PopForums.Test.Services;

public class SubscribeNotificationWorkerTests
{
	private ISubscribeNotificationRepository _subscribeNotificationRepository;
	private ISubscribedTopicsService _subscribedTopicsService;
	private INotificationAdapter _notificationAdapter;
	private IErrorLog _errorLog;

	private SubscribeNotificationWorker GetWorker()
	{
		_subscribeNotificationRepository = Substitute.For<ISubscribeNotificationRepository>();
		_subscribedTopicsService = Substitute.For<ISubscribedTopicsService>();
		_notificationAdapter = Substitute.For<INotificationAdapter>();
		_errorLog = Substitute.For<IErrorLog>();
		return new SubscribeNotificationWorker(_subscribeNotificationRepository, _subscribedTopicsService, _notificationAdapter, _errorLog);
	}

	[Fact]
	public void NoPaylodNoOtherCalls()
	{
		var worker = GetWorker();
		_subscribeNotificationRepository.Dequeue().ReturnsNull();
		
		worker.Execute();

		_subscribedTopicsService.DidNotReceiveWithAnyArgs().GetSubscribedUserIDs(Arg.Any<int>());
		_notificationAdapter.DidNotReceiveWithAnyArgs().Reply(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(),
			Arg.Any<int>(), Arg.Any<string>());
		_errorLog.DidNotReceiveWithAnyArgs().Log(Arg.Any<Exception>(), Arg.Any<ErrorSeverity>());
	}

	[Fact]
	public void PayloadValuesCallNotificationAdapter()
	{
		var worker = GetWorker();
		var payload = new SubscribeNotificationPayload { TopicID = 1, PostingUserName = "Diana", PostingUserID = 7, TopicTitle = "blah", TenantID = "pf"};
		var userIDs = new List<int> { 2, 3 };
		_subscribeNotificationRepository.Dequeue().Returns(payload);
		_subscribedTopicsService.GetSubscribedUserIDs(payload.TopicID).Returns(userIDs);
		
		worker.Execute();

		_notificationAdapter.Received().Reply(payload.PostingUserName, payload.TopicTitle, payload.TopicID, userIDs[0],
			payload.TenantID);
		_notificationAdapter.Received().Reply(payload.PostingUserName, payload.TopicTitle, payload.TopicID, userIDs[1],
			payload.TenantID);
		_errorLog.DidNotReceiveWithAnyArgs().Log(Arg.Any<Exception>(), Arg.Any<ErrorSeverity>());
	}
}