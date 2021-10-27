namespace PopForums.Repositories;

public interface IUserAwardRepository
{
	Task IssueAward(int userID, string awardDefinitionID, string title, string description, DateTime timeStamp);
	Task<bool> IsAwarded(int userID, string awardDefinitionID);
	Task<List<UserAward>> GetAwards(int userID);
}