using NSubstitute.ExceptionExtensions;

namespace PopForums.Test.Services;

public class PostImageCleanupWorkerTests
{
	private IPostImageService _postImageService;
	private IErrorLog _errorLog;

	private PostImageCleanupWorker GetWorker()
	{
		_postImageService = Substitute.For<IPostImageService>();
		_errorLog = Substitute.For<IErrorLog>();
		return new PostImageCleanupWorker(_postImageService, _errorLog);
	}

	[Fact]
	public void NoErrorNoLog()
	{
		var worker = GetWorker();
		_postImageService.DeleteOldPostImages().Returns(Task.CompletedTask);
		
		worker.Execute();
		
		_errorLog.DidNotReceive().Log(Arg.Any<Exception>(), Arg.Any<ErrorSeverity>());
	}

	[Fact]
	public void LogWhenThrows()
	{
		var worker = GetWorker(); 
		_postImageService.DeleteOldPostImages().ThrowsAsync<Exception>();
		
		worker.Execute();
		
		_errorLog.Received().Log(Arg.Any<Exception>(),ErrorSeverity.Error);
	}
}