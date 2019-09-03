using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;
using PopForums.Services;
using Xunit;

namespace PopForums.Test.ScoringGame
{
	public class AwardCalculatorTests
	{
		private AwardCalculator GetCalc()
		{
			_awardCalcRepo = new Mock<IAwardCalculationQueueRepository>();
			_eventDefService = new Mock<IEventDefinitionService>();
			_userRepo = new Mock<IUserRepository>();
			_errorLog = new Mock<IErrorLog>();
			_awardDefService = new Mock<IAwardDefinitionService>();
			_userAwardService = new Mock<IUserAwardService>();
			_pointLedgerRepo = new Mock<IPointLedgerRepository>();
			_tenantService = new Mock<ITenantService>();
			return new AwardCalculator(_awardCalcRepo.Object, _eventDefService.Object, _userRepo.Object, _errorLog.Object, _awardDefService.Object, _userAwardService.Object, _pointLedgerRepo.Object, _tenantService.Object);
		}

		private Mock<IAwardCalculationQueueRepository> _awardCalcRepo;
		private Mock<IEventDefinitionService> _eventDefService;
		private Mock<IUserRepository> _userRepo;
		private Mock<IErrorLog> _errorLog;
		private Mock<IAwardDefinitionService> _awardDefService;
		private Mock<IUserAwardService> _userAwardService;
		private Mock<IPointLedgerRepository> _pointLedgerRepo;
		private Mock<ITenantService> _tenantService;

		[Fact]
		public async Task EnqueueDoesWhatItSaysItShould()
		{
			var calc = GetCalc();
			var user = new User();
			var eventDef = new EventDefinition {EventDefinitionID = "blah"};
			var tenantID = "t1";
			_tenantService.Setup(x => x.GetTenant()).Returns(tenantID);
			var payload = new AwardCalculationPayload();
			_awardCalcRepo.Setup(x => x.Enqueue(It.IsAny<AwardCalculationPayload>())).Returns(Task.CompletedTask).Callback<AwardCalculationPayload>(a => payload = a);
			await calc.QueueCalculation(user, eventDef);
			_awardCalcRepo.Verify(x => x.Enqueue(It.IsAny<AwardCalculationPayload>()), Times.Once());
			Assert.Equal(tenantID, payload.TenantID);
			Assert.Equal(eventDef.EventDefinitionID, payload.EventDefinitionID);
		}

		[Fact]
		public async Task ProcessLogsAndDoesNothingWithNullEventDef()
		{
			var calc = GetCalc();
			var user = new User();
			_eventDefService.Setup(x => x.GetEventDefinition(It.IsAny<string>())).ReturnsAsync((EventDefinition) null);
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).ReturnsAsync(user);
			_awardCalcRepo.Setup(x => x.Dequeue()).ReturnsAsync(new KeyValuePair<string, int>("oih", user.UserID));
			await calc.ProcessCalculation(null, 0);
			_errorLog.Verify(x => x.Log(It.IsAny<Exception>(), ErrorSeverity.Warning), Times.Once());
			_userAwardService.Verify(x => x.IssueAward(It.IsAny<User>(), It.IsAny<AwardDefinition>()), Times.Never());
		}

		[Fact]
		public async Task ProcessNeverCallsIssueIfAwardedAndSingleAward()
		{
			var calc = GetCalc();
			var eventDef = new EventDefinition {EventDefinitionID = "oi"};
			var user = new User();
			var awardDef = new AwardDefinition {AwardDefinitionID = "sweet", IsSingleTimeAward = true};
			_awardCalcRepo.Setup(x => x.Dequeue()).ReturnsAsync(new KeyValuePair<string, int>(eventDef.EventDefinitionID, user.UserID));
			_eventDefService.Setup(x => x.GetEventDefinition(It.IsAny<string>())).ReturnsAsync(eventDef);
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).ReturnsAsync(user);
			_awardDefService.Setup(x => x.GetByEventDefinitionID(eventDef.EventDefinitionID)).ReturnsAsync(new List<AwardDefinition> {awardDef});
			_userAwardService.Setup(x => x.IsAwarded(user, awardDef)).ReturnsAsync(true);
			await calc.ProcessCalculation(eventDef.EventDefinitionID, user.UserID);
			_userAwardService.Verify(x => x.IssueAward(It.IsAny<User>(), It.IsAny<AwardDefinition>()), Times.Never());
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
			_awardCalcRepo.Setup(x => x.Dequeue()).ReturnsAsync(new KeyValuePair<string, int>(eventDef.EventDefinitionID, user.UserID));
			_eventDefService.Setup(x => x.GetEventDefinition(It.IsAny<string>())).ReturnsAsync(eventDef);
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).ReturnsAsync(user);
			_awardDefService.Setup(x => x.GetByEventDefinitionID(eventDef.EventDefinitionID)).ReturnsAsync(new List<AwardDefinition> { awardDef });
			_userAwardService.Setup(x => x.IsAwarded(user, awardDef)).ReturnsAsync(false);
			_awardDefService.Setup(x => x.GetConditions(awardDef.AwardDefinitionID)).ReturnsAsync(conditions);
			_pointLedgerRepo.Setup(x => x.GetEntryCount(user.UserID, conditions[0].EventDefinitionID)).ReturnsAsync(10);
			_pointLedgerRepo.Setup(x => x.GetEntryCount(user.UserID, conditions[1].EventDefinitionID)).ReturnsAsync(4);
			await calc.ProcessCalculation(eventDef.EventDefinitionID, user.UserID);
			_userAwardService.Verify(x => x.IssueAward(It.IsAny<User>(), It.IsAny<AwardDefinition>()), Times.Never());
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
			_awardCalcRepo.Setup(x => x.Dequeue()).ReturnsAsync(new KeyValuePair<string, int>(eventDef.EventDefinitionID, user.UserID));
			_eventDefService.Setup(x => x.GetEventDefinition(It.IsAny<string>())).ReturnsAsync(eventDef);
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).ReturnsAsync(user);
			_awardDefService.Setup(x => x.GetByEventDefinitionID(eventDef.EventDefinitionID)).ReturnsAsync(new List<AwardDefinition> { awardDef });
			_userAwardService.Setup(x => x.IsAwarded(user, awardDef)).ReturnsAsync(false);
			_awardDefService.Setup(x => x.GetConditions(awardDef.AwardDefinitionID)).ReturnsAsync(conditions);
			_pointLedgerRepo.Setup(x => x.GetEntryCount(user.UserID, conditions[0].EventDefinitionID)).ReturnsAsync(10);
			_pointLedgerRepo.Setup(x => x.GetEntryCount(user.UserID, conditions[1].EventDefinitionID)).ReturnsAsync(5);
			await calc.ProcessCalculation(eventDef.EventDefinitionID, user.UserID);
			_userAwardService.Verify(x => x.IssueAward(It.IsAny<User>(), It.IsAny<AwardDefinition>()), Times.Once());
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
			_awardCalcRepo.Setup(x => x.Dequeue()).ReturnsAsync(new KeyValuePair<string, int>(eventDef.EventDefinitionID, user.UserID));
			_eventDefService.Setup(x => x.GetEventDefinition(It.IsAny<string>())).ReturnsAsync(eventDef);
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).ReturnsAsync(user);
			_awardDefService.Setup(x => x.GetByEventDefinitionID(eventDef.EventDefinitionID)).ReturnsAsync(new List<AwardDefinition> { firstAwardDef, secondAwardDef });
			_userAwardService.Setup(x => x.IsAwarded(user, secondAwardDef)).ReturnsAsync(false);
			_awardDefService.Setup(x => x.GetConditions(firstAwardDef.AwardDefinitionID)).ReturnsAsync(new List<AwardCondition>());
			_awardDefService.Setup(x => x.GetConditions(secondAwardDef.AwardDefinitionID)).ReturnsAsync(conditions);
			_pointLedgerRepo.Setup(x => x.GetEntryCount(user.UserID, conditions[0].EventDefinitionID)).ReturnsAsync(10);
			_pointLedgerRepo.Setup(x => x.GetEntryCount(user.UserID, conditions[1].EventDefinitionID)).ReturnsAsync(5);
			await calc.ProcessCalculation(eventDef.EventDefinitionID, user.UserID);
			_userAwardService.Verify(x => x.IssueAward(It.IsAny<User>(), It.IsAny<AwardDefinition>()), Times.Once());
		}
	}
}
