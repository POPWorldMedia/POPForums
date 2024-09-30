using NSubstitute.ExceptionExtensions;

namespace PopForums.Test.Services;

public class UserSessionWorkerTests
{
	private IUserSessionService _userSessionService;
	private IErrorLog _errorLog;

	private UserSessionWorker GetWorker()
	{
		_userSessionService = Substitute.For<IUserSessionService>();
		_errorLog = Substitute.For<IErrorLog>();
		return new UserSessionWorker(_userSessionService, _errorLog);
	}

	[Fact]
	public void NoErrorNoLog()
	{
		var worker = GetWorker();
		_userSessionService.CleanUpExpiredSessions().Returns(Task.CompletedTask);
		
		worker.Execute();
		
		_errorLog.DidNotReceive().Log(Arg.Any<Exception>(), Arg.Any<ErrorSeverity>());
	}

	[Fact]
	public void LogWhenThrows()
	{
		var worker = GetWorker(); 
		_userSessionService.CleanUpExpiredSessions().ThrowsAsync<Exception>();
		
		worker.Execute();
		
		_errorLog.Received().Log(Arg.Any<Exception>(), ErrorSeverity.Error);
	}
}