using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.ScoringGame
{
	public interface IUserAwardService
	{
		void IssueAward(User user, AwardDefinition awardDefinition);
		bool IsAwarded(User user, AwardDefinition awardDefinition);
		List<UserAward> GetAwards(User user);
	}
}