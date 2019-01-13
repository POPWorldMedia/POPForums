using System;
using System.Collections.Generic;
using Moq;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.ScoringGame;
using Xunit;

namespace PopForums.Test.ScoringGame
{
	public class UserAwardServiceTests
	{
		public UserAwardService GetService()
		{
			_userAwardRepo = new Mock<IUserAwardRepository>();
			return new UserAwardService(_userAwardRepo.Object);
		}

		private Mock<IUserAwardRepository> _userAwardRepo;

		[Fact]
		public void IssueMapsFieldsToRepoCall()
		{
			var user = new User { UserID = 123 };
			var awardDef = new AwardDefinition {AwardDefinitionID = "blah", Description = "desc", Title = "title", IsSingleTimeAward = true};
			var service = GetService();
			service.IssueAward(user, awardDef);
			_userAwardRepo.Verify(x => x.IssueAward(user.UserID, awardDef.AwardDefinitionID, awardDef.Title, awardDef.Description, It.IsAny<DateTime>()), Times.Once());
		}

		[Fact]
		public void IsAwardedMapsAndReturnsRightValue()
		{
			var user = new User { UserID = 123 };
			var awardDef = new AwardDefinition { AwardDefinitionID = "blah" };
			var service = GetService();
			_userAwardRepo.Setup(x => x.IsAwarded(user.UserID, awardDef.AwardDefinitionID)).Returns(true);
			var result = service.IsAwarded(user, awardDef);
			Assert.True(result);
		}

		[Fact]
		public void GetAwardsMapsUserIDAndReturnsList()
		{
			var user = new User { UserID = 123 };
			var list = new List<UserAward>();
			var service = GetService();
			_userAwardRepo.Setup(x => x.GetAwards(user.UserID)).Returns(list);
			var result = service.GetAwards(user);
			Assert.Same(list, result);
		}
	}
}
