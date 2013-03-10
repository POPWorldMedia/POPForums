using System;
using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
{
	public class UserAvatarRepository : IUserAvatarRepository
	{
		public UserAvatarRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public byte[] GetImageData(int userAvatarID)
		{
			var data = new byte[] {};
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT ImageData FROM pf_UserAvatar WHERE UserAvatarID = @UserAvatarID")
					.AddParameter("@UserAvatarID", userAvatarID)
					.ExecuteReader()
					.ReadOne(r => data = (byte[])r["ImageData"]));
			return data;
		}

		public List<int> GetUserAvatarIDs(int userID)
		{
			var ids = new List<int>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT UserAvatarID FROM pf_UserAvatar WHERE UserID = @UserID")
					.AddParameter("@UserID", userID)
					.ExecuteReader()
					.ReadAll(r => ids.Add(r.GetInt32(0))));
			return ids;
		}

		public int SaveNewAvatar(int userID, byte[] imageData, DateTime timeStamp)
		{
			int avatarID = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				avatarID = Convert.ToInt32(connection.Command("INSERT INTO pf_UserAvatar (UserID, ImageData, [TimeStamp]) VALUES (@UserID, @ImageData, @TimeStamp)")
					.AddParameter("@UserID", userID)
					.AddParameter("@TimeStamp", timeStamp)
					.AddParameter("@ImageData", imageData)
					.ExecuteAndReturnIdentity()));
			return avatarID;
		}

		public void DeleteAvatarsByUserID(int userID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_UserAvatar WHERE UserID = @UserID")
					.AddParameter("@UserID", userID)
					.ExecuteNonQuery());
		}

		public DateTime? GetLastModificationDate(int userAvatarID)
		{
			DateTime? timeStamp = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT [TimeStamp] FROM pf_UserAvatar WHERE UserAvatarID = @UserAvatarID")
					.AddParameter("@UserAvatarID", userAvatarID)
					.ExecuteReader()
					.ReadOne(r => timeStamp = r.GetDateTime(0)));
			return timeStamp;
		}
	}
}
