using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.ScoringGame
{
	public interface IUserAwardService
	{
		Task IssueAward(User user, AwardDefinition awardDefinition);
		Task<bool> IsAwarded(User user, AwardDefinition awardDefinition);
		Task<List<UserAward>> GetAwards(User user);
	}

	public class UserAwardService : IUserAwardService
	{
		public UserAwardService(IUserAwardRepository userAwardRepository)
		{
			_userAwardRepository = userAwardRepository;
		}

		private readonly IUserAwardRepository _userAwardRepository;

		public async Task IssueAward(User user, AwardDefinition awardDefinition)
		{
			await _userAwardRepository.IssueAward(user.UserID, awardDefinition.AwardDefinitionID, awardDefinition.Title, awardDefinition.Description, DateTime.UtcNow);
		}

		public async Task<bool> IsAwarded(User user, AwardDefinition awardDefinition)
		{
			return await _userAwardRepository.IsAwarded(user.UserID, awardDefinition.AwardDefinitionID);
		}

		public async Task<List<UserAward>> GetAwards(User user)
		{
			return await _userAwardRepository.GetAwards(user.UserID);
		}
	}
}
