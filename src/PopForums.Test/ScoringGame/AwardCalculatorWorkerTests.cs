using NSubstitute.ExceptionExtensions;

namespace PopForums.Test.ScoringGame;

public class AwardCalculatorWorkerTests
{
	private IAwardCalculator _calculator;
	private IAwardCalculationQueueRepository _awardCalculationQueueRepository;
	private IErrorLog _errorLog;
	
	private IAwardCalculatorWorker GetWorker()
	{
		_calculator = Substitute.For<IAwardCalculator>();
		_awardCalculationQueueRepository = Substitute.For<IAwardCalculationQueueRepository>();
		_errorLog = Substitute.For<IErrorLog>();
		return new AwardCalculatorWorker(_calculator, _awardCalculationQueueRepository, _errorLog);
	}
	
	[Fact]
	public void DoNothingWhenNoPayload()
	{
		var worker = GetWorker();
		_awardCalculationQueueRepository.Dequeue().Returns(new KeyValuePair <string, int>(null, 1));
		
		worker.Execute();

		_calculator.DidNotReceiveWithAnyArgs().ProcessCalculation(Arg.Any<string>(), Arg.Any<int>());
		_errorLog.DidNotReceiveWithAnyArgs().Log(Arg.Any<Exception>(), Arg.Any<ErrorSeverity>());
	}

	[Fact]
	public void CallProcessPayloadWithPayloadValues()
	{
		var worker = GetWorker();
		_awardCalculationQueueRepository.Dequeue().Returns(new KeyValuePair <string, int>("key", 1));
		
		worker.Execute();
		
		_calculator.Received().ProcessCalculation("key", 1);
		_errorLog.DidNotReceiveWithAnyArgs().Log(Arg.Any<Exception>(), Arg.Any<ErrorSeverity>());
	}
	
	[Fact]
	public void LogWhenDequeueThrows()
	{
		var worker = GetWorker();
		_awardCalculationQueueRepository.Dequeue().ThrowsAsync(new Exception());
		
		worker.Execute();
		
		_calculator.DidNotReceiveWithAnyArgs().ProcessCalculation(Arg.Any<string>(), Arg.Any<int>());
		_errorLog.Received().Log(Arg.Any<Exception>(), ErrorSeverity.Error);
	}

	[Fact]
	public void LogWhenProcessCalculationThrows()
	{
		var worker = GetWorker();
		_awardCalculationQueueRepository.Dequeue().Returns(new KeyValuePair <string, int>("key", 1));
		_calculator.When(x => x.ProcessCalculation("key", 1)).Throw(new Exception());
		
		worker.Execute();
		
		_errorLog.Received().Log(Arg.Any<Exception>(), ErrorSeverity.Error);
	}
}