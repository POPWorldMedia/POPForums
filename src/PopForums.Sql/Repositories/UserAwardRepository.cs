using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Sql.Repositories
{
	public class UserAwardRepository : IUserAwardRepository
	{
		public UserAwardRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public virtual async Task IssueAward(int userID, string awardDefinitionID, string title, string description, DateTime timeStamp)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync(@"MERGE pf_UserAward WITH (HOLDLOCK) AS target
USING (SELECT @UserID, @AwardDefinitionID) AS source (UserID, AwardDefinitionID)
ON (target.UserID = source.UserID AND target.AwardDefinitionID = source.AwardDefinitionID)
WHEN NOT MATCHED THEN
INSERT (UserID, AwardDefinitionID, Title, Description, TimeStamp) VALUES (@UserID, @AwardDefinitionID, @Title, @Description, @TimeStamp);", new { UserID = userID, AwardDefinitionID = awardDefinitionID, Title = title, Description = description, TimeStamp = timeStamp }));
		}

		public async Task<bool> IsAwarded(int userID, string awardDefinitionID)
		{
			Task<int> count = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				count = connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM pf_UserAward WHERE UserID = @UserID AND AwardDefinitionID = @AwardDefinitionID", new { UserID = userID, AwardDefinitionID = awardDefinitionID }));
			return count.Result > 0;
		}

		public async Task<List<UserAward>> GetAwards(int userID)
		{
			Task<IEnumerable<UserAward>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<UserAward>("SELECT UserAwardID, UserID, AwardDefinitionID, Title, Description, TimeStamp FROM pf_UserAward WHERE UserID = @UserID", new { UserID = userID }));
			return list.Result.ToList();
		}
	}
}
