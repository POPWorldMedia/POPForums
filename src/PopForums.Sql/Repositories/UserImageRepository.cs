using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		public async Task<byte[]> GetImageData(int userImageID)
		{
			Task<byte[]> data = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				data = connection.ExecuteScalarAsync<byte[]>("SELECT ImageData FROM pf_UserImages WHERE UserImageID = @UserImageID", new { UserImageID = userImageID }));
			return await data;
		}

		public async Task<List<UserImage>> GetUserImages(int userID)
		{
			Task<IEnumerable<UserImage>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<UserImage>("SELECT UserImageID, UserID, SortOrder, IsApproved FROM pf_UserImages WHERE UserID = @UserID ORDER BY SortOrder", new { UserID = userID }));
			return list.Result.ToList();
		}

		public async Task<int> SaveNewImage(int userID, int sortOrder, bool isApproved, byte[] imageData, DateTime timeStamp)
		{
			Task<int> userImageID = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				userImageID = connection.QuerySingleAsync<int>("INSERT INTO pf_UserImages (UserID, SortOrder, IsApproved, [TimeStamp], ImageData) VALUES (@UserID, @SortOrder, @IsApproved, @TimeStamp, @ImageData);SELECT CAST(SCOPE_IDENTITY() as int)", new { UserID = userID, SortOrder = sortOrder, IsApproved = isApproved, TimeStamp = timeStamp, ImageData = imageData }));
			return await userImageID;
		}

		public async Task DeleteImagesByUserID(int userID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_UserImages WHERE UserID = @UserID", new { UserID = userID }));
		}

		public async Task<DateTime?> GetLastModificationDate(int userImageID)
		{
			Task<DateTime?> timeStamp = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				timeStamp = connection.QuerySingleOrDefaultAsync<DateTime?>("SELECT [TimeStamp] FROM pf_UserImages WHERE UserImageID = @UserImageID", new { UserImageID = userImageID }));
			return await timeStamp;
		}

		public async Task<List<UserImage>> GetUnapprovedUserImages()
		{
			Task<IEnumerable<UserImage>> list = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				list = connection.QueryAsync<UserImage>("SELECT UserImageID, UserID, SortOrder, IsApproved FROM pf_UserImages WHERE IsApproved = 0"));
			return list.Result.ToList();
		}

		public async Task<bool?> IsUserImageApproved(int userImageID)
		{
			Task<bool?> result = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				result = connection.QuerySingleOrDefaultAsync<bool?>("SELECT IsApproved FROM pf_UserImages WHERE UserImageID = @UserImageID", new { UserImageID = userImageID }));
			return await result;
		}

		public async Task ApproveUserImage(int userImageID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_UserImages SET IsApproved = 1 WHERE UserImageID = @UserImageID", new { UserImageID = userImageID }));
		}

		public async Task DeleteUserImage(int userImageID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("DELETE FROM pf_UserImages WHERE UserImageID = @UserImageID", new { UserImageID = userImageID }));
		}

		public async Task<UserImage> Get(int userImageID)
		{
			Task<UserImage> userImage = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				userImage = connection.QuerySingleOrDefaultAsync<UserImage>("SELECT UserImageID, UserID, SortOrder, IsApproved FROM pf_UserImages WHERE UserImageID = @UserImageID", new { UserImageID = userImageID }));
			return await userImage;
		}
	}
}
