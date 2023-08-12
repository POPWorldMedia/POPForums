namespace PopForums.Test.ScoringGame;

public class AwardCalculatorTests
{
	private AwardCalculator GetCalc()
	{
		_awardCalcRepo = Substitute.For<IAwardCalculationQueueRepository>();
		_eventDefService = Substitute.For<IEventDefinitionService>();
		_userRepo = Substitute.For<IUserRepository>();
		_errorLog = Substitute.For<IErrorLog>();
		_awardDefService = Substitute.For<IAwardDefinitionService>();
		_userAwardService = Substitute.For<IUserAwardService>();
		_pointLedgerRepo = Substitute.For<IPointLedgerRepository>();
		_tenantService = Substitute.For<ITenantService>();
		return new AwardCalculator(_awardCalcRepo, _eventDefService, _userRepo, _errorLog, _awardDefService, _userAwardService, _pointLedgerRepo, _tenantService);
	}

	private IAwardCalculationQueueRepository _awardCalcRepo;
	private IEventDefinitionService _eventDefService;
	private IUserRepository _userRepo;
	private IErrorLog _errorLog;
	private IAwardDefinitionService _awardDefService;
	private IUserAwardService _userAwardService;
	private IPointLedgerRepository _pointLedgerRepo;
	private ITenantService _tenantService;

	[Fact]
	public async Task EnqueueDoesWhatItSaysItShould()
	{
		var calc = GetCalc();
		var user = new User();
		var eventDef = new EventDefinition {EventDefinitionID = "blah"};
		var tenantID = "t1";
		_tenantService.GetTenant().Returns(tenantID);
		var payload = new AwardCalculationPayload();
		_awardCalcRepo.Enqueue(Arg.Do<AwardCalculationPayload>(x => payload = x)).Returns(Task.CompletedTask);
		await calc.QueueCalculation(user, eventDef);
		await _awardCalcRepo.Received().Enqueue(Arg.Any<AwardCalculationPayload>());
		Assert.Equal(tenantID, payload.TenantID);
		Assert.Equal(eventDef.EventDefinitionID, payload.EventDefinitionID);
	}

	[Fact]
	public async Task ProcessLogsAndDoesNothingWithNullEventDef()
	{
		var calc = GetCalc();
		var user = new User();
		_eventDefService.GetEventDefinition(Arg.Any<string>()).Returns((EventDefinition) null);
		_userRepo.GetUser(Arg.Any<int>()).Returns(Task.FromResult(user));
		_awardCalcRepo.Dequeue().Returns(Task.FromResult(new KeyValuePair<string, int>("oih", user.UserID)));
		await calc.ProcessCalculation(null, 0);
		_errorLog.Received().Log(Arg.Any<Exception>(), ErrorSeverity.Warning);
		await _userAwardService.DidNotReceive().IssueAward(Arg.Any<User>(), Arg.Any<AwardDefinition>());
	}

	[Fact]
	public async Task ProcessNeverCallsIssueIfAwardedAndSingleAward()
	{
		var calc = GetCalc();
		var eventDef = new EventDefinition {EventDefinitionID = "oi"};
		var user = new User();
		var awardDef = new AwardDefinition {AwardDefinitionID = "sweet", IsSingleTimeAward = true};
		_awardCalcRepo.Dequeue().Returns(Task.FromResult(new KeyValuePair<string, int>(eventDef.EventDefinitionID, user.UserID)));
		_eventDefService.GetEventDefinition(Arg.Any<string>()).Returns(Task.FromResult(eventDef));
		_userRepo.GetUser(Arg.Any<int>()).Returns(Task.FromResult(user));
		_awardDefService.GetByEventDefinitionID(eventDef.EventDefinitionID).Returns(Task.FromResult(new List<AwardDefinition> {awardDef}));
		_userAwardService.IsAwarded(user, awardDef).Returns(Task.FromResult(true));
		await calc.ProcessCalculation(eventDef.EventDefinitionID, user.UserID);
		await _userAwardService.DidNotReceive().IssueAward(Arg.Any<User>(), Arg.Any<AwardDefinition>());
	}

	[Fact]
	public async Task ProcessNeverCallsIfEventCountNotHighEnough()
	{
		var calc = GetCalc();
		var eventDef = new EventDefinition { EventDefinitionID = "oi" };
		var user = new User { UserID = 1 };
		var awardDef = new AwardDefinition {AwardDefinitionID = "sweet", IsSingleTimeAward = true};
		var conditions = new List<AwardCondition>
		{
			new AwardCondition { AwardDefinitionID = awardDef.AwardDefinitionID, EventDefinitionID ="qwerty", EventCount = 3},
			new AwardCondition { AwardDefinitionID = awardDef.AwardDefinitionID, EventDefinitionID ="asdfgh", EventCount = 5}
		};
		_awardCalcRepo.Dequeue().Returns(Task.FromResult(new KeyValuePair<string, int>(eventDef.EventDefinitionID, user.UserID)));
		_eventDefService.GetEventDefinition(Arg.Any<string>()).Returns(Task.FromResult(eventDef));
		_userRepo.GetUser(Arg.Any<int>()).Returns(Task.FromResult(user));
		_awardDefService.GetByEventDefinitionID(eventDef.EventDefinitionID).Returns(Task.FromResult(new List<AwardDefinition> { awardDef }));
		_userAwardService.IsAwarded(user, awardDef).Returns(Task.FromResult(false));
		_awardDefService.GetConditions(awardDef.AwardDefinitionID).Returns(Task.FromResult(conditions));
		_pointLedgerRepo.GetEntryCount(user.UserID, conditions[0].EventDefinitionID).Returns(Task.FromResult(10));
		_pointLedgerRepo.GetEntryCount(user.UserID, conditions[1].EventDefinitionID).Returns(Task.FromResult(4));
		await calc.ProcessCalculation(eventDef.EventDefinitionID, user.UserID);
		await _userAwardService.DidNotReceive().IssueAward(Arg.Any<User>(), Arg.Any<AwardDefinition>());
	}

	[Fact]
	public async Task ProcessIssuesAwardWhenConditionsEqualOrGreater()
	{
		var calc = GetCalc();
		var eventDef = new EventDefinition { EventDefinitionID = "oi" };
		var user = new User { UserID = 1 };
		var awardDef = new AwardDefinition { AwardDefinitionID = "sweet", IsSingleTimeAward = true };
		var conditions = new List<AwardCondition>
		{
			new AwardCondition { AwardDefinitionID = awardDef.AwardDefinitionID, EventDefinitionID ="qwerty", EventCount = 3},
			new AwardCondition { AwardDefinitionID = awardDef.AwardDefinitionID, EventDefinitionID ="asdfgh", EventCount = 5}
		};
		_awardCalcRepo.Dequeue().Returns(Task.FromResult(new KeyValuePair<string, int>(eventDef.EventDefinitionID, user.UserID)));
		_eventDefService.GetEventDefinition(Arg.Any<string>()).Returns(Task.FromResult(eventDef));
		_userRepo.GetUser(Arg.Any<int>()).Returns(Task.FromResult(user));
		_awardDefService.GetByEventDefinitionID(eventDef.EventDefinitionID).Returns(Task.FromResult(new List<AwardDefinition> { awardDef }));
		_userAwardService.IsAwarded(user, awardDef).Returns(Task.FromResult(false));
		_awardDefService.GetConditions(awardDef.AwardDefinitionID).Returns(Task.FromResult(conditions));
		_pointLedgerRepo.GetEntryCount(user.UserID, conditions[0].EventDefinitionID).Returns(Task.FromResult(10));
		_pointLedgerRepo.GetEntryCount(user.UserID, conditions[1].EventDefinitionID).Returns(Task.FromResult(5));
		await calc.ProcessCalculation(eventDef.EventDefinitionID, user.UserID);
		await _userAwardService.Received().IssueAward(Arg.Any<User>(), Arg.Any<AwardDefinition>());
	}

	[Fact]
	public async Task ProcessIssuesSecondAwardWhenConditionsEqualOrGreater()
	{
		var calc = GetCalc();
		var eventDef = new EventDefinition { EventDefinitionID = "oi" };
		var user = new User { UserID = 1 };
		var firstAwardDef = new AwardDefinition {AwardDefinitionID = "first", IsSingleTimeAward = true};
		var secondAwardDef = new AwardDefinition { AwardDefinitionID = "sweet", IsSingleTimeAward = true };
		var conditions = new List<AwardCondition>
		{
			new AwardCondition { AwardDefinitionID = secondAwardDef.AwardDefinitionID, EventDefinitionID ="qwerty", EventCount = 3},
			new AwardCondition { AwardDefinitionID = secondAwardDef.AwardDefinitionID, EventDefinitionID ="asdfgh", EventCount = 5}
		};
		_awardCalcRepo.Dequeue().Returns(Task.FromResult(new KeyValuePair<string, int>(eventDef.EventDefinitionID, user.UserID)));
		_eventDefService.GetEventDefinition(Arg.Any<string>()).Returns(Task.FromResult(eventDef));
		_userRepo.GetUser(Arg.Any<int>()).Returns(Task.FromResult(user));
		_awardDefService.GetByEventDefinitionID(eventDef.EventDefinitionID).Returns(Task.FromResult(new List<AwardDefinition> { firstAwardDef, secondAwardDef }));
		_userAwardService.IsAwarded(user, secondAwardDef).Returns(Task.FromResult(false));
		_awardDefService.GetConditions(firstAwardDef.AwardDefinitionID).Returns(Task.FromResult(new List<AwardCondition>()));
		_awardDefService.GetConditions(secondAwardDef.AwardDefinitionID).Returns(Task.FromResult(conditions));
		_pointLedgerRepo.GetEntryCount(user.UserID, conditions[0].EventDefinitionID).Returns(Task.FromResult(10));
		_pointLedgerRepo.GetEntryCount(user.UserID, conditions[1].EventDefinitionID).Returns(Task.FromResult(5));
		await calc.ProcessCalculation(eventDef.EventDefinitionID, user.UserID);
		await _userAwardService.Received().IssueAward(Arg.Any<User>(), Arg.Any<AwardDefinition>());
	}
}