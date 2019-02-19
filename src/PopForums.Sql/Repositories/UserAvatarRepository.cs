using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
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
			byte[] data = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				data = connection.ExecuteScalar<byte[]>("SELECT ImageData FROM pf_UserAvatar WHERE UserAvatarID = @UserAvatarID", new { UserAvatarID = userAvatarID }));
			return data;
		}

		public List<int> GetUserAvatarIDs(int userID)
		{
			List<int> ids = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				ids = connection.Query<int>("SELECT UserAvatarID FROM pf_UserAvatar WHERE UserID = @UserID",
					new {UserID = userID}).ToList());
			return ids;
		}

		public int SaveNewAvatar(int userID, byte[] imageData, DateTime timeStamp)
		{
			int avatarID = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				avatarID = connection.QuerySingle<int>("INSERT INTO pf_UserAvatar (UserID, ImageData, [TimeStamp]) VALUES (@UserID, @ImageData, @TimeStamp);SELECT CAST(SCOPE_IDENTITY() as int)", new { UserID = userID, TimeStamp = timeStamp, ImageData = imageData }));
			return avatarID;
		}

		public void DeleteAvatarsByUserID(int userID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_UserAvatar WHERE UserID = @UserID", new { UserID = userID }));
		}

		public DateTime? GetLastModificationDate(int userAvatarID)
		{
			DateTime? timeStamp = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				timeStamp = connection.QuerySingleOrDefault<DateTime?>("SELECT [TimeStamp] FROM pf_UserAvatar WHERE UserAvatarID = @UserAvatarID", new { UserAvatarID = userAvatarID }));
			return timeStamp;
		}
	}
}
