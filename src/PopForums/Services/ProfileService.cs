using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IProfileService
	{
		Task<Profile> GetProfile(User user);
		Task Create(Profile profile);
		Task<Profile> Create(User user, SignupData signupData);
		Task Update(Profile profile);
		Task<Profile> GetProfileForEdit(User user);
		Task<Dictionary<int, string>> GetSignatures(List<Post> posts);
		Task<Dictionary<int, int>> GetAvatars(List<Post> posts);
		Task SetCurrentImageIDToNull(int userID);
		string GetUnsubscribeHash(User user);
		Task<bool> Unsubscribe(User user, string hash);
		Task UpdatePointTotal(User user);
	}

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

		public async Task<Profile> GetProfile(User user)
		{
			if (user == null)
				return null;
			return await _profileRepository.GetProfile(user.UserID);
		}

		public async Task<Profile> GetProfileForEdit(User user)
		{
			var profile = await _profileRepository.GetProfile(user.UserID);
			if (string.IsNullOrWhiteSpace(profile.Signature))
				profile.Signature = string.Empty;
			else
				profile.Signature = _textParsingService.ClientHtmlToForumCode(profile.Signature);
			return profile;
		}

		public async Task Create(Profile profile)
		{
			if (profile.UserID == 0)
				throw new Exception("Can't create a profile not associated with a valid UserID");
			await _profileRepository.Create(profile);
		}

		public async Task<Profile> Create(User user, SignupData signupData)
		{
			var profile = new Profile
            {
				UserID = user.UserID,
	            TimeZone = signupData.TimeZone,
	            IsDaylightSaving = signupData.IsDaylightSaving,
	            IsSubscribed = signupData.IsSubscribed,
	            IsTos = signupData.IsTos
            };
			await _profileRepository.Create(profile);
			return profile;
		}

		public async Task Update(Profile profile)
		{
			profile.Signature = profile.Signature.Trim();
			if (await _profileRepository.Update(profile) == false)
				throw new Exception($"Profile with UserID {profile.UserID} does not exist.");
		}

		public async Task<Dictionary<int, string>> GetSignatures(List<Post> posts)
		{
			var userIDs = posts.Where(p => p.ShowSig).Select(p => p.UserID).Distinct().ToList();
			return await _profileRepository.GetSignatures(userIDs);
		}

		public async Task<Dictionary<int, int>> GetAvatars(List<Post> posts)
		{
			var userIDs = posts.Select(p => p.UserID).Distinct().ToList();
			return await _profileRepository.GetAvatars(userIDs);
		}

		public async Task SetCurrentImageIDToNull(int userID)
		{
			await _profileRepository.SetCurrentImageIDToNull(userID);
		}

		public string GetUnsubscribeHash(User user)
		{
			var source = user.Name + user.Email;
			return source.GetSHA256Hash();
		}

		public async Task<bool> Unsubscribe(User user, string hash)
		{
			if (GetUnsubscribeHash(user) != hash)
				return false;
			var profile = await GetProfile(user);
			profile.IsSubscribed = false;
			await Update(profile);
			return true;
		}

		public async Task UpdatePointTotal(User user)
		{
			var total = await _pointLedgerRepository.GetPointTotal(user.UserID);
			await _profileRepository.UpdatePoints(user.UserID, total);
		}
	}
}