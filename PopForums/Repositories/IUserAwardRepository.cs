using System;
using System.Collections.Generic;
using PopForums.ScoringGame;

namespace PopForums.Repositories
{
	public interface IUserAwardRepository
	{
		void IssueAward(int userID, string awardDefinitionID, string title, string description, DateTime timeStamp);
		bool IsAwarded(int userID, string awardDefinitionID);
		List<UserAward> GetAwards(int userID);
	}
}
