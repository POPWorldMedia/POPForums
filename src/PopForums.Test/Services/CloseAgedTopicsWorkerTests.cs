using NSubstitute.ExceptionExtensions;

namespace PopForums.Test.Services;

public class CloseAgedTopicsWorkerTests
{
	private ITopicService _topicService;
	private IErrorLog _errorLog;

	private CloseAgedTopicsWorker GetWorker()
	{
		_topicService = Substitute.For<ITopicService>();
		_errorLog = Substitute.For<IErrorLog>();
		return new CloseAgedTopicsWorker(_topicService, _errorLog);
	}

	[Fact]
	public void NoErrorNoLog()
	{
		var worker = GetWorker();
		_topicService.CloseAgedTopics().Returns(Task.CompletedTask);
		
		worker.Execute();
		
		_errorLog.DidNotReceive().Log(Arg.Any<Exception>(), Arg.Any<ErrorSeverity>());
	}

	[Fact]
	public void LogWhenThrows()
	{
		var worker = GetWorker(); 
		_topicService.CloseAgedTopics().ThrowsAsync<Exception>();
		
		worker.Execute();
		
		_errorLog.Received().Log(Arg.Any<Exception>(), Arg.Any<ErrorSeverity>());
	}
}