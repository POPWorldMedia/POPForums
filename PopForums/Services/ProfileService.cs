using System;
using System.Collections.Generic;
using System.Linq;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public class ProfileService : IProfileService
	{
		private readonly IProfileRepository _profileRepository;
		private readonly ITextParsingService _textParsingService;
		private readonly IPointLedgerRepository _pointLedgerRepository;

		public ProfileService(IProfileRepository profileRepository, ITextParsingService textParsingService, IPointLedgerRepository pointLedgerRepository)
		{
			_profileRepository = profileRepository;
			_textParsingService = textParsingService;
			_pointLedgerRepository = pointLedgerRepository;
		}

		public Profile GetProfile(User user)
		{
			if (user == null)
				return null;
			return _profileRepository.GetProfile(user.UserID);
		}

		public Profile GetProfileForEdit(User user)
		{
			var profile = _profileRepository.GetProfile(user.UserID);
			profile.Signature = _textParsingService.ClientHtmlToForumCode(profile.Signature);
			return profile;
		}

		public void Create(Profile profile)
		{
			if (profile.UserID == 0)
				throw new Exception("Can't create a profile not associated with a valid UserID");
			_profileRepository.Create(profile);
		}

		public Profile Create(User user, SignupData signupData)
		{
			var profile = new Profile(user.UserID)
			              	{
			              		TimeZone = signupData.TimeZone,
			              		IsDaylightSaving = signupData.IsDaylightSaving,
			              		IsSubscribed = signupData.IsSubscribed,
			              		IsTos = signupData.IsTos
			              	};
			_profileRepository.Create(profile);
			return profile;
		}

		public void Update(Profile profile)
		{
			profile.Signature = profile.Signature.Trim();
			if (!_profileRepository.Update(profile))
				throw new Exception(String.Format("Profile with UserID {0} does not exist.", profile.UserID));
		}

		public Dictionary<int, string> GetSignatures(List<Post> posts)
		{
			var userIDs = posts.Where(p => p.ShowSig).Select(p => p.UserID).Distinct().ToList();
			return _profileRepository.GetSignatures(userIDs);
		}

		public Dictionary<int, int> GetAvatars(List<Post> posts)
		{
			var userIDs = posts.Select(p => p.UserID).Distinct().ToList();
			return _profileRepository.GetAvatars(userIDs);
		}

		public void SetCurrentImageIDToNull(int userID)
		{
			_profileRepository.SetCurrentImageIDToNull(userID);
		}

		public string GetUnsubscribeHash(User user)
		{
			var source = user.Name + user.Email;
			return source.GetMD5Hash();
		}

		public bool Unsubscribe(User user, string hash)
		{
			if (GetUnsubscribeHash(user) != hash)
				return false;
			var profile = GetProfile(user);
			profile.IsSubscribed = false;
			Update(profile);
			return true;
		}

		public void UpdatePointTotal(User user)
		{
			var total = _pointLedgerRepository.GetPointTotal(user.UserID);
			_profileRepository.UpdatePoints(user.UserID, total);
		}
	}
}