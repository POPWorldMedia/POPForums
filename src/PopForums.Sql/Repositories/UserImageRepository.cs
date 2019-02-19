using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class UserImageRepository : IUserImageRepository
	{
		public UserImageRepository(ISqlObjectFactory sqlObjectFactory)
		{
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ISqlObjectFactory _sqlObjectFactory;

		public byte[] GetImageData(int userImageID)
		{
			byte[] data = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				data = connection.ExecuteScalar<byte[]>("SELECT ImageData FROM pf_UserImages WHERE UserImageID = @UserImageID", new { UserImageID = userImageID }));
			return data;
		}

		public List<UserImage> GetUserImages(int userID)
		{
			List<UserImage> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<UserImage>("SELECT UserImageID, UserID, SortOrder, IsApproved FROM pf_UserImages WHERE UserID = @UserID ORDER BY SortOrder", new { UserID = userID }).ToList());
			return list;
		}

		public int SaveNewImage(int userID, int sortOrder, bool isApproved, byte[] imageData, DateTime timeStamp)
		{
			int userImageID = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				userImageID = connection.QuerySingle<int>("INSERT INTO pf_UserImages (UserID, SortOrder, IsApproved, [TimeStamp], ImageData) VALUES (@UserID, @SortOrder, @IsApproved, @TimeStamp, @ImageData);SELECT CAST(SCOPE_IDENTITY() as int)", new { UserID = userID, SortOrder = sortOrder, IsApproved = isApproved, TimeStamp = timeStamp, ImageData = imageData }));
			return userImageID;
		}

		public void DeleteImagesByUserID(int userID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_UserImages WHERE UserID = @UserID", new { UserID = userID }));
		}

		public DateTime? GetLastModificationDate(int userImageID)
		{
			DateTime? timeStamp = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				timeStamp = connection.QuerySingleOrDefault<DateTime?>("SELECT [TimeStamp] FROM pf_UserImages WHERE UserImageID = @UserImageID", new { UserImageID = userImageID }));
			return timeStamp;
		}

		public List<UserImage> GetUnapprovedUserImages()
		{
			List<UserImage> list = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				list = connection.Query<UserImage>("SELECT UserImageID, UserID, SortOrder, IsApproved FROM pf_UserImages WHERE IsApproved = 0").ToList());
			return list;
		}

		public bool? IsUserImageApproved(int userImageID)
		{
			bool? result = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				result = connection.QuerySingleOrDefault<bool?>("SELECT IsApproved FROM pf_UserImages WHERE UserImageID = @UserImageID", new { UserImageID = userImageID }));
			return result;
		}

		public void ApproveUserImage(int userImageID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("UPDATE pf_UserImages SET IsApproved = 1 WHERE UserImageID = @UserImageID", new { UserImageID = userImageID }));
		}

		public void DeleteUserImage(int userImageID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("DELETE FROM pf_UserImages WHERE UserImageID = @UserImageID", new { UserImageID = userImageID }));
		}

		public UserImage Get(int userImageID)
		{
			UserImage userImage = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				userImage = connection.QuerySingleOrDefault<UserImage>("SELECT UserImageID, UserID, SortOrder, IsApproved FROM pf_UserImages WHERE UserImageID = @UserImageID", new { UserImageID = userImageID }));
			return userImage;
		}
	}
}
