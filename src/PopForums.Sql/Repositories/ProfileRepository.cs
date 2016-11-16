using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Data.Sql.Repositories
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

		private static Profile PopulateFromReader(DbDataReader reader)
		{
			return new Profile(reader.GetInt32(0)) {
				IsSubscribed = reader.GetBoolean(1),
				Signature = reader.GetString(2),
				ShowDetails = reader.GetBoolean(3),
				Location = reader.GetString(4),
				IsPlainText = reader.GetBoolean(5),
				Dob = reader.NullDateTimeDbHelper(6),
				Web = reader.GetString(7),
				Aim = reader.GetString(8),
				Icq = reader.GetString(9),
				YahooMessenger = reader.GetString(10),
				Facebook = reader.NullStringDbHelper(11),
				Twitter = reader.NullStringDbHelper(12),
				IsTos = reader.GetBoolean(13),
				TimeZone = reader.GetInt32(14),
				IsDaylightSaving = reader.GetBoolean(15),
				AvatarID = reader.NullIntDbHelper(16),
				ImageID = reader.NullIntDbHelper(17),
				HideVanity = reader.GetBoolean(18),
				LastPostID = reader.NullIntDbHelper(19),
				Points = reader.GetInt32(20)
			};
		}

		public Profile GetProfile(int userID)
		{
			var cacheObject = _cacheHelper.GetCacheObject<Profile>(CacheKeys.UserProfile(userID));
			if (cacheObject != null)
				return cacheObject;
			Profile profile = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command(_sqlObjectFactory, "SELECT UserID, IsSubscribed, Signature, ShowDetails, Location, IsPlainText, DOB, Web, AIM, ICQ, YahooMessenger, Facebook, Twitter, IsTos, TimeZone, IsDaylightSaving, AvatarID, ImageID, HideVanity, LastPostID, Points FROM pf_Profile WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@UserID", userID)
					.ExecuteReader()
					.ReadOne(r => { profile = PopulateFromReader(r); }));
			if (profile != null)
				_cacheHelper.SetCacheObject(CacheKeys.UserProfile(userID), profile);
			return profile;
		}

		public void Create(Profile profile)
		{
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command(_sqlObjectFactory, "INSERT INTO pf_Profile (UserID, IsSubscribed, Signature, ShowDetails, Location, IsPlainText, DOB, Web, AIM, ICQ, YahooMessenger, Facebook, Twitter, IsTos, TimeZone, IsDaylightSaving, AvatarID, ImageID, HideVanity, LastPostID, Points) VALUES (@UserID, @IsSubscribed, @Signature, @ShowDetails, @Location, @IsPlainText, @DOB, @Web, @AIM, @ICQ, @YahooMessenger, @Facebook, @Twitter, @IsTos, @TimeZone, @IsDaylightSaving, @AvatarID, @ImageID, @HideVanity, @LastPostID, @Points)")
					.AddParameter(_sqlObjectFactory, "@UserID", profile.UserID)
					.AddParameter(_sqlObjectFactory, "@IsSubscribed", profile.IsSubscribed)
					.AddParameter(_sqlObjectFactory, "@Signature", profile.Signature.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@ShowDetails", profile.ShowDetails)
					.AddParameter(_sqlObjectFactory, "@Location", profile.Location.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@IsPlainText", profile.IsPlainText)
					.AddParameter(_sqlObjectFactory, "@DOB", profile.Dob.GetObjectOrDbNull())
					.AddParameter(_sqlObjectFactory, "@Web", profile.Web.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@AIM", profile.Aim.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@ICQ", profile.Icq.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@YahooMessenger", profile.YahooMessenger.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@Facebook", profile.Facebook.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@Twitter", profile.Twitter.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@IsTos", profile.IsTos)
					.AddParameter(_sqlObjectFactory, "@TimeZone", profile.TimeZone)
					.AddParameter(_sqlObjectFactory, "@IsDaylightSaving", profile.IsDaylightSaving)
					.AddParameter(_sqlObjectFactory, "@AvatarID", profile.AvatarID.GetObjectOrDbNull())
					.AddParameter(_sqlObjectFactory, "@ImageID", profile.ImageID.GetObjectOrDbNull())
					.AddParameter(_sqlObjectFactory, "@HideVanity", profile.HideVanity)
					.AddParameter(_sqlObjectFactory, "@LastPostID", profile.LastPostID.GetObjectOrDbNull())
					.AddParameter(_sqlObjectFactory, "@Points", profile.Points)
					.ExecuteNonQuery());
		}

		public bool Update(Profile profile)
		{
			var success = false;
			_sqlObjectFactory.GetConnection().Using(connection => 
				success = connection.Command(_sqlObjectFactory, "UPDATE pf_Profile SET IsSubscribed = @IsSubscribed, Signature = @Signature, ShowDetails = @ShowDetails, Location = @Location, IsPlainText = @IsPlainText, DOB = @DOB, Web = @Web, AIM = @AIM, ICQ = @ICQ, YahooMessenger = @YahooMessenger, Facebook = @Facebook, Twitter = @Twitter, IsTos = @IsTos, TimeZone = @TimeZone, IsDaylightSaving = @IsDaylightSaving, AvatarID = @AvatarID, ImageID = @ImageID, HideVanity = @HideVanity, LastPostID = @LastPostID, Points = @Points WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@IsSubscribed", profile.IsSubscribed)
					.AddParameter(_sqlObjectFactory, "@Signature", profile.Signature.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@ShowDetails", profile.ShowDetails)
					.AddParameter(_sqlObjectFactory, "@Location", profile.Location.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@IsPlainText", profile.IsPlainText)
					.AddParameter(_sqlObjectFactory, "@DOB", profile.Dob.GetObjectOrDbNull())
					.AddParameter(_sqlObjectFactory, "@Web", profile.Web.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@AIM", profile.Aim.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@ICQ", profile.Icq.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@YahooMessenger", profile.YahooMessenger.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@Facebook", profile.Facebook.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@Twitter", profile.Twitter.NullToEmpty())
					.AddParameter(_sqlObjectFactory, "@IsTos", profile.IsTos)
					.AddParameter(_sqlObjectFactory, "@TimeZone", profile.TimeZone)
					.AddParameter(_sqlObjectFactory, "@IsDaylightSaving", profile.IsDaylightSaving)
					.AddParameter(_sqlObjectFactory, "@AvatarID", profile.AvatarID.GetObjectOrDbNull())
					.AddParameter(_sqlObjectFactory, "@ImageID", profile.ImageID.GetObjectOrDbNull())
					.AddParameter(_sqlObjectFactory, "@HideVanity", profile.HideVanity)
					.AddParameter(_sqlObjectFactory, "@LastPostID", profile.LastPostID.GetObjectOrDbNull())
					.AddParameter(_sqlObjectFactory, "@Points", profile.Points)
					.AddParameter(_sqlObjectFactory, "@UserID", profile.UserID)
					.ExecuteNonQuery() == 1);
			_cacheHelper.RemoveCacheObject(CacheKeys.UserProfile(profile.UserID));
			return success;
		}

		public int? GetLastPostID(int userID)
		{
			int? postID = null;
			_sqlObjectFactory.GetConnection().Using(connection => 
				connection.Command(_sqlObjectFactory, "SELECT LastPostID FROM pf_Profile WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@UserID", userID)
					.ExecuteReader()
					.ReadOne(r => { postID = r.NullIntDbHelper(0); }));
			return postID;
		}

		public bool SetLastPostID(int userID, int postID)
		{
			var success = false;
			_sqlObjectFactory.GetConnection().Using(connection => 
				success = connection.Command(_sqlObjectFactory, "UPDATE pf_Profile SET LastPostID = @LastPostID WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@LastPostID", postID)
					.AddParameter(_sqlObjectFactory, "@UserID", userID).ExecuteNonQuery() == 1);
			return success;
		}

		public Dictionary<int, string> GetSignatures(List<int> userIDs)
		{
			var dictionary = new Dictionary<int, string>();
			if (userIDs.Count == 0)
				return dictionary;
			var inList = userIDs.Aggregate(String.Empty, (current, userID) => current + ("," + userID));
			if (inList.StartsWith(","))
				inList = inList.Remove(0, 1);
			var sql = String.Format("SELECT UserID, Signature FROM pf_Profile WHERE NOT Signature = '' AND UserID IN ({0})", inList);
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, sql)
					.ExecuteReader()
					.ReadAll(r => dictionary.Add(r.GetInt32(0), r.GetString(1))));
			return dictionary;
		}

		public Dictionary<int, int> GetAvatars(List<int> userIDs)
		{
			var dictionary = new Dictionary<int, int>();
			if (userIDs.Count == 0)
				return dictionary;
			var inList = userIDs.Aggregate(String.Empty, (current, userID) => current + ("," + userID));
			if (inList.StartsWith(","))
				inList = inList.Remove(0, 1);
			var sql = String.Format("SELECT UserID, AvatarID FROM pf_Profile WHERE NOT AvatarID IS NULL AND UserID IN ({0})", inList);
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, sql)
					.ExecuteReader()
					.ReadAll(r => dictionary.Add(r.GetInt32(0), r.GetInt32(1))));
			return dictionary;
		}

		public void SetCurrentImageIDToNull(int userID)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "UPDATE pf_Profile SET ImageID = NULL WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@UserID", userID)
					.ExecuteNonQuery());
		}

		public void UpdatePoints(int userID, int points)
		{
			_sqlObjectFactory.GetConnection().Using(connection =>
				connection.Command(_sqlObjectFactory, "UPDATE pf_Profile SET Points = @Points WHERE UserID = @UserID")
					.AddParameter(_sqlObjectFactory, "@UserID", userID)
					.AddParameter(_sqlObjectFactory, "@Points", points)
					.ExecuteNonQuery());
			_cacheHelper.RemoveCacheObject(CacheKeys.UserProfile(userID));
		}
	}
}
