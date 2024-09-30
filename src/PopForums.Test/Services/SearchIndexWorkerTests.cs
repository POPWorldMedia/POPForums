using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace PopForums.Test.Services;

public class SearchIndexWorkerTests
{
	private IErrorLog _errorLog;
	private ISearchIndexSubsystem _searchIndexSubsystem;
	private ISearchService _searchService;
	
	private ISearchIndexWorker GetWorker()
	{
		_errorLog = Substitute.For<IErrorLog>();
		_searchIndexSubsystem = Substitute.For<ISearchIndexSubsystem>();
		_searchService = Substitute.For<ISearchService>();
		return new SearchIndexWorker(_errorLog, _searchIndexSubsystem, _searchService);
	}
	
	[Fact]
	public void DoNothingWhenNoPayload()
	{
		var worker = GetWorker();
		_searchService.GetNextTopicForIndexing().ReturnsNull();
		
		worker.Execute();

		_searchIndexSubsystem.DidNotReceiveWithAnyArgs().DoIndex(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>());
		_errorLog.DidNotReceiveWithAnyArgs().Log(Arg.Any<Exception>(), Arg.Any<ErrorSeverity>());
	}
	
	[Fact]
	public void CallSearchIndexSubsystemWhenPayload()
	{
		var worker = GetWorker();
		var payload = new SearchIndexPayload { TopicID = 123, TenantID = "tenant", IsForRemoval = true };
		_searchService.GetNextTopicForIndexing().Returns(payload);
		
		worker.Execute();

		_searchIndexSubsystem.Received().DoIndex(123, "tenant", true);
		_errorLog.DidNotReceiveWithAnyArgs().Log(Arg.Any<Exception>(), Arg.Any<ErrorSeverity>());
	}

	[Fact]
	public void LogWhenGetNextTopicForIndexingThrows()
	{
		var worker = GetWorker();
		_searchService.GetNextTopicForIndexing().ThrowsAsync(new Exception());
		
		worker.Execute();
		
		_searchIndexSubsystem.DidNotReceiveWithAnyArgs().DoIndex(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>());
		_errorLog.Received().Log(Arg.Any<Exception>(), ErrorSeverity.Error);
	}
	
	[Fact]
	public void LogWhenDoIndexThrows()
	{
		var worker = GetWorker();
		var payload = new SearchIndexPayload { TopicID = 123, TenantID = "tenant", IsForRemoval = true };
		_searchService.GetNextTopicForIndexing().Returns(payload);
		_searchIndexSubsystem.When(x => x.DoIndex(123, "tenant", true)).Throw(new Exception());
		
		worker.Execute();
		
		_searchIndexSubsystem.Received().DoIndex(123, "tenant", true);
		_errorLog.Received().Log(Arg.Any<Exception>(), ErrorSeverity.Error);
	}
}