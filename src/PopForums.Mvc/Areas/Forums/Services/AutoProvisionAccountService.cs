using Microsoft.Extensions.Logging;
using PopForums.Configuration;
using PopForums.Extensions;
using PopForums.ExternalLogin;
using PopForums.Models;
using PopForums.Services;
using System;
using System.Threading.Tasks;

namespace PopForums.Mvc.Areas.Forums.Services
{
	public interface IAutoProvisionAccountService
	{
		Task<User> AutoProvisionAccountAsync(string username, string email, string ip);
	}

	public class AutoProvisionAccountService : IAutoProvisionAccountService
	{
		private readonly ISettingsManager _settingsManager;
		private readonly IExternalUserAssociationManager _externalUserAssociationManager;
		private readonly IUserService _userService;
		private readonly IProfileService _profileService;
		private readonly ISecurityLogService _securityLogService;
		private readonly IUserRetrievalShim _userRetrievalShim;
		private readonly IConfig _config;
		private readonly ILogger<AutoProvisionAccountService> _log;

		public AutoProvisionAccountService(ISettingsManager settingsManager, IExternalUserAssociationManager externalUserAssociationManager, IUserService userService, IProfileService profileService, ISecurityLogService securityLogService, IUserRetrievalShim userRetrievalShim, IConfig config, ILogger<AutoProvisionAccountService> log)
		{
			_settingsManager = settingsManager;
			_externalUserAssociationManager = externalUserAssociationManager;
			_userService = userService;
			_profileService = profileService;
			_securityLogService = securityLogService;
			_userRetrievalShim = userRetrievalShim;
			_config = config;
			_log = log;
		}

		public async Task<User> AutoProvisionAccountAsync(string username, string email, string ip)
		{
			var user = await _userService.GetUserByEmail(email);

			if (user == null)
			{
				username = await EnsureUniqueUsernameAsync(username);

				user = await CreateUserAsync(username, email, ip);

				_log.LogInformation($"Created new user for external email '{email}'.");
			}
			else
			{
				// Update the username to match the external login. If the username is already in use, generate a random one.
				if (user.Name != username)
				{
					username = await EnsureUniqueUsernameAsync(username);

					await _userService.ChangeName(user, username, user, ip);
				}

				_log.LogInformation($"Linking user to external email '{email}'.");
			}

			await CopyExternalUserPropertiesAsync(user, ip);

			return user;
		}

		private async Task<User> CreateUserAsync(string username, string email, string ip)
		{
			var password = Guid.NewGuid().ToString().GetSHA256Hash();

			var user = await _userService.CreateUser(username, email, password, true, ip);

			var profile = new Profile
			{
				UserID = user.UserID,
				TimeZone = _settingsManager.Current.ServerTimeZone,
				IsDaylightSaving = true,
				IsSubscribed = true,
			};

			await _profileService.Create(profile);

			return user;
		}

		private async Task<string> EnsureUniqueUsernameAsync(string username)
		{
			if (await _userService.GetUserByName(username) != null)
			{
				_log.LogWarning($"Local username already exists for external username '{username}'.");

				username += DateTime.Now.Ticks.ToString();
			}

			return username;
		}

		protected virtual async Task CopyExternalUserPropertiesAsync(User user, string ip)
		{
		}
	}
}
