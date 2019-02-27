using System;
using System.Collections.Generic;
using System.Linq;
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

		public virtual void IssueAward(int userID, string awardDefinitionID, string title, string description, DateTime timeStamp)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("INSERT INTO pf_UserAward (UserID, AwardDefinitionID, Title, Description, TimeStamp) VALUES (@UserID, @AwardDefinitionID, @Title, @Description, @TimeStamp)", new { UserID = userID, AwardDefinitionID = awardDefinitionID, Title = title, Description = description, TimeStamp = timeStamp }));
		}

		public bool IsAwarded(int userID, string awardDefinitionID)
		{
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM pf_UserAward WHERE UserID = @UserID AND AwardDefinitionID = @AwardDefinitionID", new { UserID = userID, AwardDefinitionID = awardDefinitionID }));
			return count > 0;
		}

		public List<UserAward> GetAwards(int userID)
		{
			List<UserAward> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<UserAward>("SELECT UserAwardID, UserID, AwardDefinitionID, Title, Description, TimeStamp FROM pf_UserAward WHERE UserID = @UserID", new { UserID = userID }).ToList());
			return list;
		}
	}
}
