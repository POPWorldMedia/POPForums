using System;
using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.SqlSingleWebServer.Repositories
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
			var data = new byte[] { };
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT ImageData FROM pf_UserImages WHERE UserImageID = @UserImageID")
					.AddParameter("@UserImageID", userImageID)
					.ExecuteReader()
					.ReadOne(r => data = (byte[])r["ImageData"]));
			return data;
		}

		public List<UserImage> GetUserImages(int userID)
		{
			var list = new List<UserImage>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT UserImageID, UserID, SortOrder, IsApproved FROM pf_UserImages WHERE UserID = @UserID ORDER BY SortOrder")
					.AddParameter("@UserID", userID)
					.ExecuteReader()
					.ReadAll(r => list.Add(new UserImage {UserImageID = r.GetInt32(0), UserID = r.GetInt32(1), SortOrder = r.GetInt32(2), IsApproved = r.GetBoolean(3)})));
			return list;
		}

		public int SaveNewImage(int userID, int sortOrder, bool isApproved, byte[] imageData, DateTime timeStamp)
		{
			int userImageID = 0;
			_sqlObjectFactory.GetConnection().Using(connection =>
				userImageID = Convert.ToInt32(connection.Command("INSERT INTO pf_UserImages (UserID, SortOrder, IsApproved, [TimeStamp], ImageData) VALUES (@UserID, @SortOrder, @IsApproved, @TimeStamp, @ImageData)")
					.AddParameter("@UserID", userID)
					.AddParameter("@SortOrder", sortOrder)
					.AddParameter("@IsApproved", isApproved)
					.AddParameter("@TimeStamp", timeStamp)
					.AddParameter("@ImageData", imageData)
					.ExecuteAndReturnIdentity()));
			return userImageID;
		}

		public void DeleteImagesByUserID(int userID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_UserImages WHERE UserID = @UserID")
					.AddParameter("@UserID", userID)
					.ExecuteNonQuery());
		}

		public DateTime? GetLastModificationDate(int userImageID)
		{
			DateTime? timeStamp = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT [TimeStamp] FROM pf_UserImages WHERE UserImageID = @UserImageID")
					.AddParameter("@UserImageID", userImageID)
					.ExecuteReader()
					.ReadOne(r => timeStamp = r.GetDateTime(0)));
			return timeStamp;
		}

		public List<UserImage> GetUnapprovedUserImages()
		{
			var list = new List<UserImage>();
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT UserImageID, UserID, SortOrder, IsApproved FROM pf_UserImages WHERE IsApproved = 0")
					.ExecuteReader()
					.ReadAll(r => list.Add(new UserImage { UserImageID = r.GetInt32(0), UserID = r.GetInt32(1), SortOrder = r.GetInt32(2), IsApproved = r.GetBoolean(3) })));
			return list;
		}

		public bool? IsUserImageApproved(int userImageID)
		{
			bool? result = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
			                                 connection.Command("SELECT IsApproved FROM pf_UserImages WHERE UserImageID = @UserImageID")
			                                 	.AddParameter("@UserImageID", userImageID)
			                                 	.ExecuteReader()
			                                 	.ReadOne(r => result = r.GetBoolean(0)));
			return result;
		}

		public void ApproveUserImage(int userImageID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("UPDATE pf_UserImages SET IsApproved = 1 WHERE UserImageID = @UserImageID")
					.AddParameter("@UserImageID", userImageID)
					.ExecuteNonQuery());
		}

		public void DeleteUserImage(int userImageID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("DELETE FROM pf_UserImages WHERE UserImageID = @UserImageID")
					.AddParameter("@UserImageID", userImageID)
					.ExecuteNonQuery());
		}

		public UserImage Get(int userImageID)
		{
			UserImage userImage = null;
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command("SELECT UserImageID, UserID, SortOrder, IsApproved FROM pf_UserImages WHERE UserImageID = @UserImageID")
					.AddParameter("@UserImageID", userImageID)
					.ExecuteReader()
					.ReadOne(r => userImage = new UserImage { UserImageID = r.GetInt32(0), UserID = r.GetInt32(1), SortOrder = r.GetInt32(2), IsApproved = r.GetBoolean(3) }));
			return userImage;
		}
	}
}
