using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Test.ScoringGame
{
	[TestFixture]
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
			return new AwardCalculator(_awardCalcRepo.Object, _eventDefService.Object, _userRepo.Object, _errorLog.Object, _awardDefService.Object, _userAwardService.Object, _pointLedgerRepo.Object);
		}

		private Mock<IAwardCalculationQueueRepository> _awardCalcRepo;
		private Mock<IEventDefinitionService> _eventDefService;
		private Mock<IUserRepository> _userRepo;
		private Mock<IErrorLog> _errorLog;
		private Mock<IAwardDefinitionService> _awardDefService;
		private Mock<IUserAwardService> _userAwardService;
		private Mock<IPointLedgerRepository> _pointLedgerRepo;

		[Test]
		public void EnqueueDoesWhatItSaysItShould()
		{
			var calc = GetCalc();
			var user = new User(1, DateTime.MinValue);
			var eventDef = new EventDefinition {EventDefinitionID = "blah"};
			calc.QueueCalculation(user, eventDef);
			_awardCalcRepo.Verify(x => x.Enqueue(eventDef.EventDefinitionID, user.UserID), Times.Once());
		}

		[Test]
		public void ProcessLogsAndDoesNothingWithNullEventDef()
		{
			var calc = GetCalc();
			var user = new User(1, DateTime.MinValue);
			_eventDefService.Setup(x => x.GetEventDefinition(It.IsAny<string>())).Returns((EventDefinition) null);
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).Returns(user);
			_awardCalcRepo.Setup(x => x.Dequeue()).Returns(new KeyValuePair<string, int>("oih", user.UserID));
			calc.ProcessOneCalculation();
			_errorLog.Verify(x => x.Log(It.IsAny<Exception>(), ErrorSeverity.Warning), Times.Once());
			_userAwardService.Verify(x => x.IssueAward(It.IsAny<User>(), It.IsAny<AwardDefinition>()), Times.Never());
		}

		[Test]
		public void ProcessLogsAndDoesNothingWithNullUser()
		{
			var calc = GetCalc();
			var eventDef = new EventDefinition {EventDefinitionID = "oi"};
			_eventDefService.Setup(x => x.GetEventDefinition(It.IsAny<string>())).Returns(eventDef);
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).Returns((User)null);
			_awardCalcRepo.Setup(x => x.Dequeue()).Returns(new KeyValuePair<string, int>(eventDef.EventDefinitionID, 123));
			calc.ProcessOneCalculation();
			_errorLog.Verify(x => x.Log(It.IsAny<Exception>(), ErrorSeverity.Warning), Times.Once());
			_userAwardService.Verify(x => x.IssueAward(It.IsAny<User>(), It.IsAny<AwardDefinition>()), Times.Never());
		}

		[Test]
		public void ProcessNeverCallsIssueIfAwardedAndSingleAward()
		{
			var calc = GetCalc();
			var eventDef = new EventDefinition {EventDefinitionID = "oi"};
			var user = new User(1, DateTime.MinValue);
			var awardDef = new AwardDefinition {AwardDefinitionID = "sweet", IsSingleTimeAward = true};
			_awardCalcRepo.Setup(x => x.Dequeue()).Returns(new KeyValuePair<string, int>(eventDef.EventDefinitionID, user.UserID));
			_eventDefService.Setup(x => x.GetEventDefinition(It.IsAny<string>())).Returns(eventDef);
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).Returns(user);
			_awardDefService.Setup(x => x.GetByEventDefinitionID(eventDef.EventDefinitionID)).Returns(new List<AwardDefinition> {awardDef});
			_userAwardService.Setup(x => x.IsAwarded(user, awardDef)).Returns(true);
			calc.ProcessOneCalculation();
			_userAwardService.Verify(x => x.IssueAward(It.IsAny<User>(), It.IsAny<AwardDefinition>()), Times.Never());
		}

		[Test]
		public void ProcessNeverCallsIfEventCountNotHighEnough()
		{
			var calc = GetCalc();
			var eventDef = new EventDefinition { EventDefinitionID = "oi" };
			var user = new User(1, DateTime.MinValue);
			var awardDef = new AwardDefinition {AwardDefinitionID = "sweet", IsSingleTimeAward = true};
			var conditions = new List<AwardCondition>
			        {
			            new AwardCondition { AwardDefinitionID = awardDef.AwardDefinitionID, EventDefinitionID ="qwerty", EventCount = 3},
			            new AwardCondition { AwardDefinitionID = awardDef.AwardDefinitionID, EventDefinitionID ="asdfgh", EventCount = 5}
			        };
			_awardCalcRepo.Setup(x => x.Dequeue()).Returns(new KeyValuePair<string, int>(eventDef.EventDefinitionID, user.UserID));
			_eventDefService.Setup(x => x.GetEventDefinition(It.IsAny<string>())).Returns(eventDef);
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).Returns(user);
			_awardDefService.Setup(x => x.GetByEventDefinitionID(eventDef.EventDefinitionID)).Returns(new List<AwardDefinition> { awardDef });
			_userAwardService.Setup(x => x.IsAwarded(user, awardDef)).Returns(false);
			_awardDefService.Setup(x => x.GetConditions(awardDef.AwardDefinitionID)).Returns(conditions);
			_pointLedgerRepo.Setup(x => x.GetEntryCount(user.UserID, conditions[0].EventDefinitionID)).Returns(10);
			_pointLedgerRepo.Setup(x => x.GetEntryCount(user.UserID, conditions[1].EventDefinitionID)).Returns(4);
			calc.ProcessOneCalculation();
			_userAwardService.Verify(x => x.IssueAward(It.IsAny<User>(), It.IsAny<AwardDefinition>()), Times.Never());
		}

		[Test]
		public void ProcessIssuesAwardWhenConditionsEqualOrGreater()
		{
			var calc = GetCalc();
			var eventDef = new EventDefinition { EventDefinitionID = "oi" };
			var user = new User(1, DateTime.MinValue);
			var awardDef = new AwardDefinition { AwardDefinitionID = "sweet", IsSingleTimeAward = true };
			var conditions = new List<AwardCondition>
			        {
			            new AwardCondition { AwardDefinitionID = awardDef.AwardDefinitionID, EventDefinitionID ="qwerty", EventCount = 3},
			            new AwardCondition { AwardDefinitionID = awardDef.AwardDefinitionID, EventDefinitionID ="asdfgh", EventCount = 5}
			        };
			_awardCalcRepo.Setup(x => x.Dequeue()).Returns(new KeyValuePair<string, int>(eventDef.EventDefinitionID, user.UserID));
			_eventDefService.Setup(x => x.GetEventDefinition(It.IsAny<string>())).Returns(eventDef);
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).Returns(user);
			_awardDefService.Setup(x => x.GetByEventDefinitionID(eventDef.EventDefinitionID)).Returns(new List<AwardDefinition> { awardDef });
			_userAwardService.Setup(x => x.IsAwarded(user, awardDef)).Returns(false);
			_awardDefService.Setup(x => x.GetConditions(awardDef.AwardDefinitionID)).Returns(conditions);
			_pointLedgerRepo.Setup(x => x.GetEntryCount(user.UserID, conditions[0].EventDefinitionID)).Returns(10);
			_pointLedgerRepo.Setup(x => x.GetEntryCount(user.UserID, conditions[1].EventDefinitionID)).Returns(5);
			calc.ProcessOneCalculation();
			_userAwardService.Verify(x => x.IssueAward(It.IsAny<User>(), It.IsAny<AwardDefinition>()), Times.Once());
		}

		[Test]
		public void ProcessIssuesSecondAwardWhenConditionsEqualOrGreater()
		{
			var calc = GetCalc();
			var eventDef = new EventDefinition { EventDefinitionID = "oi" };
			var user = new User(1, DateTime.MinValue);
			var firstAwardDef = new AwardDefinition {AwardDefinitionID = "first", IsSingleTimeAward = true};
			var secondAwardDef = new AwardDefinition { AwardDefinitionID = "sweet", IsSingleTimeAward = true };
			var conditions = new List<AwardCondition>
			        {
			            new AwardCondition { AwardDefinitionID = secondAwardDef.AwardDefinitionID, EventDefinitionID ="qwerty", EventCount = 3},
			            new AwardCondition { AwardDefinitionID = secondAwardDef.AwardDefinitionID, EventDefinitionID ="asdfgh", EventCount = 5}
			        };
			_awardCalcRepo.Setup(x => x.Dequeue()).Returns(new KeyValuePair<string, int>(eventDef.EventDefinitionID, user.UserID));
			_eventDefService.Setup(x => x.GetEventDefinition(It.IsAny<string>())).Returns(eventDef);
			_userRepo.Setup(x => x.GetUser(It.IsAny<int>())).Returns(user);
			_awardDefService.Setup(x => x.GetByEventDefinitionID(eventDef.EventDefinitionID)).Returns(new List<AwardDefinition> { firstAwardDef, secondAwardDef });
			_userAwardService.Setup(x => x.IsAwarded(user, secondAwardDef)).Returns(false);
			_awardDefService.Setup(x => x.GetConditions(firstAwardDef.AwardDefinitionID)).Returns(new List<AwardCondition>());
			_awardDefService.Setup(x => x.GetConditions(secondAwardDef.AwardDefinitionID)).Returns(conditions);
			_pointLedgerRepo.Setup(x => x.GetEntryCount(user.UserID, conditions[0].EventDefinitionID)).Returns(10);
			_pointLedgerRepo.Setup(x => x.GetEntryCount(user.UserID, conditions[1].EventDefinitionID)).Returns(5);
			calc.ProcessOneCalculation();
			_userAwardService.Verify(x => x.IssueAward(It.IsAny<User>(), It.IsAny<AwardDefinition>()), Times.Once());
		}
	}
}
