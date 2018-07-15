using System;
using System.Collections.Generic;
using PopForums.Configuration;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface ISetupService
	{
		bool IsRuntimeConnectionAndSetupGood();
		bool IsConnectionPossible();
		bool IsDatabaseSetup();
		User SetupDatabase(SetupVariables setupVariables, out Exception exception);
	}

	public class SetupService : ISetupService
	{
		public SetupService(ISetupRepository setupRepository, IUserService userService, ISettingsManager settingsManager, IProfileService profileService)
		{
			_setupRepository = setupRepository;
			_userService = userService;
			_settingsManager = settingsManager;
			_profileService = profileService;
		}

		private readonly ISetupRepository _setupRepository;
		private readonly IUserService _userService;
		private readonly ISettingsManager _settingsManager;
		private readonly IProfileService _profileService;

		private static bool? _isConnectionSetupGood;

		public bool IsRuntimeConnectionAndSetupGood()
		{
			if (_isConnectionSetupGood.HasValue && _isConnectionSetupGood.Value)
				return true;
			_isConnectionSetupGood = _setupRepository.IsConnectionPossible();
			if (!_isConnectionSetupGood.Value)
				return false;
			_isConnectionSetupGood = _setupRepository.IsDatabaseSetup();
			return _isConnectionSetupGood.Value;
		}

		public bool IsConnectionPossible()
		{
			return _setupRepository.IsConnectionPossible();
		}

		public bool IsDatabaseSetup()
		{
			return _setupRepository.IsDatabaseSetup();
		}

		public User SetupDatabase(SetupVariables setupVariables, out Exception exception)
		{
			exception = null;
			try
			{
				_setupRepository.SetupDatabase();
			}
			catch (Exception exc)
			{
				exception = exc;
				return null;
			}

			var settings = _settingsManager.Current;
			settings.ForumTitle = setupVariables.ForumTitle;
			settings.SmtpServer = setupVariables.SmtpServer;
			settings.SmtpPort = setupVariables.SmtpPort;
			settings.MailerAddress = setupVariables.MailerAddress;
			settings.UseSslSmtp = setupVariables.UseSslSmtp;
			settings.UseEsmtp = setupVariables.UseEsmtp;
			settings.SmtpUser = setupVariables.SmtpUser;
			settings.SmtpPassword = setupVariables.SmtpPassword;
			_settingsManager.SaveCurrent();

			var user = _userService.CreateUser(setupVariables.Name, setupVariables.Email, setupVariables.Password, true, "");
			user.Roles = new List<string> {PermanentRoles.Admin, PermanentRoles.Moderator};
			var profile = new Profile(user.UserID) { IsTos = true, IsSubscribed = true, TimeZone = setupVariables.ServerTimeZone, IsDaylightSaving = setupVariables.ServerDaylightSaving, ShowDetails = true };
			_profileService.Create(profile);
			var edit = new UserEdit(user, profile);
			_userService.EditUser(user, edit, false, false, null, null, "", user);
			//PopForumsActivation.StartServicesIfRunningInstance();
			return user;
		}
	}
}
