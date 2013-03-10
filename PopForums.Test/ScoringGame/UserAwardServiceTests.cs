using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Test.ScoringGame
{
	[TestFixture]
	public class UserAwardServiceTests
	{
		public UserAwardService GetService()
		{
			_userAwardRepo = new Mock<IUserAwardRepository>();
			return new UserAwardService(_userAwardRepo.Object);
		}

		private Mock<IUserAwardRepository> _userAwardRepo;

		[Test]
		public void IssueMapsFieldsToRepoCall()
		{
			var user = new User(123, DateTime.MinValue);
			var awardDef = new AwardDefinition {AwardDefinitionID = "blah", Description = "desc", Title = "title", IsSingleTimeAward = true};
			var service = GetService();
			service.IssueAward(user, awardDef);
			_userAwardRepo.Verify(x => x.IssueAward(user.UserID, awardDef.AwardDefinitionID, awardDef.Title, awardDef.Description, It.IsAny<DateTime>()), Times.Once());
		}

		[Test]
		public void IsAwardedMapsAndReturnsRightValue()
		{
			var user = new User(123, DateTime.MinValue);
			var awardDef = new AwardDefinition { AwardDefinitionID = "blah" };
			var service = GetService();
			_userAwardRepo.Setup(x => x.IsAwarded(user.UserID, awardDef.AwardDefinitionID)).Returns(true);
			var result = service.IsAwarded(user, awardDef);
			Assert.IsTrue(result);
		}

		[Test]
		public void GetAwardsMapsUserIDAndReturnsList()
		{
			var user = new User(123, DateTime.MinValue);
			var list = new List<UserAward>();
			var service = GetService();
			_userAwardRepo.Setup(x => x.GetAwards(user.UserID)).Returns(list);
			var result = service.GetAwards(user);
			Assert.AreSame(list, result);
		}
	}
}
