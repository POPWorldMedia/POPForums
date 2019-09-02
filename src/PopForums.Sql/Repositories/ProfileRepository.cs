using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Sql.Repositories
{
	public class ProfileRepository : IProfileRepository
	{
		public ProfileRepository(ICacheHelper cacheHelper, ISqlObjectFactory sqlObjectFactory)
		{
			_cacheHelper = cacheHelper;
			_sqlObjectFactory = sqlObjectFactory;
		}

		private readonly ICacheHelper _cacheHelper;
		private readonly ISqlObjectFactory _sqlObjectFactory;

		public class CacheKeys
		{
			public static string UserProfile(int userID)
			{
				return "PopForums.Profile.User." + userID;
			}
		}

		public async Task<Profile> GetProfile(int userID)
		{
			var key = CacheKeys.UserProfile(userID);
			var cachedItem = _cacheHelper.GetCacheObject<Profile>(key);
			if (cachedItem != null)
				return cachedItem;
			Task<Profile> profile = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				profile = connection.QuerySingleOrDefaultAsync<Profile>("SELECT UserID, IsSubscribed, Signature, ShowDetails, Location, IsPlainText, DOB, Web, Facebook, Twitter, Instagram, IsTos, TimeZone, IsDaylightSaving, AvatarID, ImageID, HideVanity, LastPostID, Points FROM pf_Profile WHERE UserID = @UserID", new { UserID = userID }));
			if (profile.Result != null)
				_cacheHelper.SetCacheObject(key, profile.Result);
			return await profile;
		}

		public async Task Create(Profile profile)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				connection.ExecuteAsync("INSERT INTO pf_Profile (UserID, IsSubscribed, Signature, ShowDetails, Location, IsPlainText, DOB, Web, Facebook, Twitter, Instagram, IsTos, TimeZone, IsDaylightSaving, AvatarID, ImageID, HideVanity, LastPostID, Points) VALUES (@UserID, @IsSubscribed, @Signature, @ShowDetails, @Location, @IsPlainText, @DOB, @Web, @Facebook, @Twitter, @Instagram, @IsTos, @TimeZone, @IsDaylightSaving, @AvatarID, @ImageID, @HideVanity, @LastPostID, @Points)", new { profile.UserID, profile.IsSubscribed, Signature = profile.Signature.NullToEmpty(), profile.ShowDetails, Location = profile.Location.NullToEmpty(), profile.IsPlainText, profile.Dob, Web = profile.Web.NullToEmpty(), Instagram =  profile.Instagram.NullToEmpty(), Facebook = profile.Facebook.NullToEmpty(), Twitter = profile.Twitter.NullToEmpty(), profile.IsTos, profile.TimeZone, profile.IsDaylightSaving, profile.AvatarID, profile.ImageID, profile.HideVanity, profile.LastPostID, profile.Points }));
		}

		public async Task<bool> Update(Profile profile)
		{
			Task<int> success = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				success = connection.ExecuteAsync("UPDATE pf_Profile SET IsSubscribed = @IsSubscribed, Signature = @Signature, ShowDetails = @ShowDetails, Location = @Location, IsPlainText = @IsPlainText, DOB = @DOB, Web = @Web, Facebook = @Facebook, Twitter = @Twitter, Instagram = @Instagram, IsTos = @IsTos, TimeZone = @TimeZone, IsDaylightSaving = @IsDaylightSaving, AvatarID = @AvatarID, ImageID = @ImageID, HideVanity = @HideVanity, LastPostID = @LastPostID, Points = @Points WHERE UserID = @UserID", new { profile.UserID, profile.IsSubscribed, Signature = profile.Signature.NullToEmpty(), profile.ShowDetails, Location = profile.Location.NullToEmpty(), profile.IsPlainText, profile.Dob, Web = profile.Web.NullToEmpty(), Instagram = profile.Instagram.NullToEmpty(), Facebook = profile.Facebook.NullToEmpty(), Twitter = profile.Twitter.NullToEmpty(), profile.IsTos, profile.TimeZone, profile.IsDaylightSaving, profile.AvatarID, profile.ImageID, profile.HideVanity, profile.LastPostID, profile.Points }));
			_cacheHelper.RemoveCacheObject(CacheKeys.UserProfile(profile.UserID));
			return success.Result == 1;
		}

		public async Task<int?> GetLastPostID(int userID)
		{
			Task<int?> postID = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				postID = connection.QuerySingleOrDefaultAsync<int?>("SELECT LastPostID FROM pf_Profile WHERE UserID = @UserID", new { UserID = userID }));
			return await postID;
		}

		public async Task<bool> SetLastPostID(int userID, int postID)
		{
			Task<int> success = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection => 
				success = connection.ExecuteAsync("UPDATE pf_Profile SET LastPostID = @LastPostID WHERE UserID = @UserID", new { LastPostID = postID, UserID = userID }));
			return success.Result == 1;
		}

		public async Task<Dictionary<int, string>> GetSignatures(List<int> userIDs)
		{
			if (userIDs.Count == 0)
				return new Dictionary<int, string>();
			var inList = userIDs.Aggregate(string.Empty, (current, userID) => current + ("," + userID));
			if (inList.StartsWith(","))
				inList = inList.Remove(0, 1);
			var sql = $"SELECT UserID, Signature FROM pf_Profile WHERE NOT Signature = '' AND UserID IN ({inList})";
			Task<IEnumerable<dynamic>> dictionary = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				dictionary = connection.QueryAsync(sql));
			return dictionary.Result.ToDictionary(r => (int)r.UserID, r => (string)r.Signature);
		}

		public async Task<Dictionary<int, int>> GetAvatars(List<int> userIDs)
		{
			if (userIDs.Count == 0)
				return new Dictionary<int, int>();
			var inList = userIDs.Aggregate(string.Empty, (current, userID) => current + ("," + userID));
			if (inList.StartsWith(","))
				inList = inList.Remove(0, 1);
			var sql = $"SELECT UserID, AvatarID FROM pf_Profile WHERE NOT AvatarID IS NULL AND UserID IN ({inList})";
			Task<IEnumerable<dynamic>> dictionary = null;
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				dictionary = connection.QueryAsync(sql));
			return dictionary.Result.ToDictionary(r => (int)r.UserID, r => (int)r.AvatarID);
		}

		public async Task SetCurrentImageIDToNull(int userID)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_Profile SET ImageID = NULL WHERE UserID = @UserID", new { UserID = userID }));
			_cacheHelper.RemoveCacheObject(CacheKeys.UserProfile(userID));
		}

		public async Task UpdatePoints(int userID, int points)
		{
			await _sqlObjectFactory.GetConnection().UsingAsync(connection =>
				connection.ExecuteAsync("UPDATE pf_Profile SET Points = @Points WHERE UserID = @UserID", new { UserID = userID, Points = points }));
			_cacheHelper.RemoveCacheObject(CacheKeys.UserProfile(userID));
		}
	}
}
