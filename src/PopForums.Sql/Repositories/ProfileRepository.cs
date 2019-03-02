using System;
using System.Collections.Generic;
using System.Linq;
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

		public Profile GetProfile(int userID)
		{
			Profile profile = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				profile = connection.QuerySingleOrDefault<Profile>("SELECT UserID, IsSubscribed, Signature, ShowDetails, Location, IsPlainText, DOB, Web, Facebook, Twitter, Instagram, IsTos, TimeZone, IsDaylightSaving, AvatarID, ImageID, HideVanity, LastPostID, Points FROM pf_Profile WHERE UserID = @UserID", new { UserID = userID }));
			return profile;
		}

		public void Create(Profile profile)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Execute("INSERT INTO pf_Profile (UserID, IsSubscribed, Signature, ShowDetails, Location, IsPlainText, DOB, Web, Facebook, Twitter, Instagram, IsTos, TimeZone, IsDaylightSaving, AvatarID, ImageID, HideVanity, LastPostID, Points) VALUES (@UserID, @IsSubscribed, @Signature, @ShowDetails, @Location, @IsPlainText, @DOB, @Web, @Facebook, @Twitter, @Instagram, @IsTos, @TimeZone, @IsDaylightSaving, @AvatarID, @ImageID, @HideVanity, @LastPostID, @Points)", new { profile.UserID, profile.IsSubscribed, Signature = profile.Signature.NullToEmpty(), profile.ShowDetails, Location = profile.Location.NullToEmpty(), profile.IsPlainText, profile.Dob, Web = profile.Web.NullToEmpty(), Instagram =  profile.Instagram.NullToEmpty(), Facebook = profile.Facebook.NullToEmpty(), Twitter = profile.Twitter.NullToEmpty(), profile.IsTos, profile.TimeZone, profile.IsDaylightSaving, profile.AvatarID, profile.ImageID, profile.HideVanity, profile.LastPostID, profile.Points }));
		}

		public bool Update(Profile profile)
		{
			var success = false;
			_sqlObjectFactory.GetConnection().Using(connection => 
				success = connection.Execute("UPDATE pf_Profile SET IsSubscribed = @IsSubscribed, Signature = @Signature, ShowDetails = @ShowDetails, Location = @Location, IsPlainText = @IsPlainText, DOB = @DOB, Web = @Web, Facebook = @Facebook, Twitter = @Twitter, Instagram = @Instagram, IsTos = @IsTos, TimeZone = @TimeZone, IsDaylightSaving = @IsDaylightSaving, AvatarID = @AvatarID, ImageID = @ImageID, HideVanity = @HideVanity, LastPostID = @LastPostID, Points = @Points WHERE UserID = @UserID", new { profile.UserID, profile.IsSubscribed, Signature = profile.Signature.NullToEmpty(), profile.ShowDetails, Location = profile.Location.NullToEmpty(), profile.IsPlainText, profile.Dob, Web = profile.Web.NullToEmpty(), Instagram = profile.Instagram.NullToEmpty(), Facebook = profile.Facebook.NullToEmpty(), Twitter = profile.Twitter.NullToEmpty(), profile.IsTos, profile.TimeZone, profile.IsDaylightSaving, profile.AvatarID, profile.ImageID, profile.HideVanity, profile.LastPostID, profile.Points }) == 1);
			_cacheHelper.RemoveCacheObject(CacheKeys.UserProfile(profile.UserID));
			return success;
		}

		public int? GetLastPostID(int userID)
		{
			int? postID = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				postID = connection.QuerySingle<int?>("SELECT LastPostID FROM pf_Profile WHERE UserID = @UserID", new { UserID = userID }));
			return postID;
		}

		public bool SetLastPostID(int userID, int postID)
		{
			var success = false;
			_sqlObjectFactory.GetConnection().Using(connection => 
				success = connection.Execute("UPDATE pf_Profile SET LastPostID = @LastPostID WHERE UserID = @UserID", new { LastPostID = postID, UserID = userID }) == 1);
			return success;
		}

		public Dictionary<int, string> GetSignatures(List<int> userIDs)
		{
			Dictionary<int, string> dictionary = null;
			if (userIDs.Count == 0)
				return new Dictionary<int, string>();
			var inList = userIDs.Aggregate(String.Empty, (current, userID) => current + ("," + userID));
			if (inList.StartsWith(","))
				inList = inList.Remove(0, 1);
			var sql = $"SELECT UserID, Signature FROM pf_Profile WHERE NOT Signature = '' AND UserID IN ({inList})";
			_sqlObjectFactory.GetConnection().Using(connection =>
				dictionary = connection.Query(sql).ToDictionary(r => (int)r.UserID, r => (string)r.Signature));
			return dictionary;
		}

		public Dictionary<int, int> GetAvatars(List<int> userIDs)
		{
			Dictionary<int, int> dictionary = null;
			if (userIDs.Count == 0)
				return new Dictionary<int, int>();
			var inList = userIDs.Aggregate(String.Empty, (current, userID) => current + ("," + userID));
			if (inList.StartsWith(","))
				inList = inList.Remove(0, 1);
			var sql = $"SELECT UserID, AvatarID FROM pf_Profile WHERE NOT AvatarID IS NULL AND UserID IN ({inList})";
			_sqlObjectFactory.GetConnection().Using(connection =>
				dictionary = connection.Query(sql).ToDictionary(r => (int)r.UserID, r => (int)r.AvatarID));
			return dictionary;
		}

		public void SetCurrentImageIDToNull(int userID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("UPDATE pf_Profile SET ImageID = NULL WHERE UserID = @UserID", new { UserID = userID }));
			_cacheHelper.RemoveCacheObject(CacheKeys.UserProfile(userID));
		}

		public void UpdatePoints(int userID, int points)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Execute("UPDATE pf_Profile SET Points = @Points WHERE UserID = @UserID", new { UserID = userID, Points = points }));
			_cacheHelper.RemoveCacheObject(CacheKeys.UserProfile(userID));
		}
	}
}
