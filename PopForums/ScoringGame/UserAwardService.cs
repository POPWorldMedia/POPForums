using System;
using System.Collections.Generic;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.ScoringGame
{
	public class UserAwardService : IUserAwardService
	{
		public UserAwardService(IUserAwardRepository userAwardRepository)
		{
			_userAwardRepository = userAwardRepository;
		}

		private readonly IUserAwardRepository _userAwardRepository;

		public void IssueAward(User user, AwardDefinition awardDefinition)
		{
			_userAwardRepository.IssueAward(user.UserID, awardDefinition.AwardDefinitionID, awardDefinition.Title, awardDefinition.Description, DateTime.UtcNow);
		}

		public bool IsAwarded(User user, AwardDefinition awardDefinition)
		{
			return _userAwardRepository.IsAwarded(user.UserID, awardDefinition.AwardDefinitionID);
		}

		public List<UserAward> GetAwards(User user)
		{
			return _userAwardRepository.GetAwards(user.UserID);
		}
	}
}
