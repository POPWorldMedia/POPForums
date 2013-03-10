using System;
using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Repositories;
using PopForums.ScoringGame;

namespace PopForums.Data.SqlSingleWebServer.Repositories
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
				connection.Command("INSERT INTO pf_UserAward (UserID, AwardDefinitionID, Title, Description, TimeStamp) VALUES (@UserID, @AwardDefinitionID, @Title, @Description, @TimeStamp)")
				.AddParameter("@UserID", userID)
				.AddParameter("@AwardDefinitionID", awardDefinitionID)
				.AddParameter("@Title", title)
				.AddParameter("@Description", description)
				.AddParameter("@TimeStamp", timeStamp)
				.ExecuteNonQuery());
		}

		public bool IsAwarded(int userID, string awardDefinitionID)
		{
			var count = 0;
			_sqlObjectFactory.GetConnection().Using(connection => count = Convert.ToInt32(
				connection.Command("SELECT COUNT(*) FROM pf_UserAward WHERE UserID = @UserID AND AwardDefinitionID = @AwardDefinitionID")
				.AddParameter("@UserID", userID)
				.AddParameter("@AwardDefinitionID", awardDefinitionID)
				.ExecuteScalar()));
			return count > 0;
		}

		public List<UserAward> GetAwards(int userID)
		{
			var list = new List<UserAward>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT UserAwardID, UserID, AwardDefinitionID, Title, Description, TimeStamp FROM pf_UserAward WHERE UserID = @UserID")
				.AddParameter("@UserID", userID)
				.ExecuteReader()
				.ReadAll(r => list.Add(new UserAward
				                       	{
				                       		UserAwardID = r.GetInt32(0),
											UserID = r.GetInt32(1),
											AwardDefinitionID = r.GetString(2),
											Title = r.GetString(3),
											Description = r.GetString(4),
											TimeStamp = r.GetDateTime(5)
				                       	})));
			return list;
		}
	}
}
