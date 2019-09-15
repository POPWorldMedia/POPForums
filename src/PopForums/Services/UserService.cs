using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PopForums.Configuration;
using PopForums.Email;
using PopForums.Extensions;
using PopForums.Models;
using PopForums.Repositories;

namespace PopForums.Services
{
	public interface IUserService
	{
		Task SetPassword(User targetUser, string password, string ip, User user);
		Task<Tuple<bool, Guid?>> CheckPassword(string email, string password);
		Task<User> GetUser(int userID);
		Task<User> GetUserByName(string name);
		Task<User> GetUserByEmail(string email);
		Task<User> GetUserByAuhtorizationKey(Guid authorizationKey);
		Task<bool> IsNameInUse(string name);
		Task<bool> IsEmailInUse(string email);
		Task<User> CreateUser(SignupData signupData, string ip);
		Task<User> CreateUser(string name, string email, string password, bool isApproved, string ip);
		Task DeleteUser(User targetUser, User user, string ip, bool ban);
		Task UpdateLastActivityDate(User user);
		Task ChangeEmail(User targetUser, string newEmail, User user, string ip);
		Task ChangeEmail(User targetUser, string newEmail, User user, string ip, bool isUserApproved);
		Task ChangeName(User targetUser, string newName, User user, string ip);
		Task UpdateIsApproved(User targetUser, bool isApproved, User user, string ip);
		Task UpdateAuthorizationKey(User user, Guid key);
		Task Logout(User user, string ip);
		Task<Tuple<bool, User>> Login(string email, string password, string ip);
		Task Login(User user, string ip);
		Task<List<string>> GetAllRoles();
		Task CreateRole(string role, User user, string ip);
		Task DeleteRole(string role, User user, string ip);
		Task<User> VerifyAuthorizationCode(Guid key, string ip);
		Task<List<User>> SearchByEmail(string email);
		Task<List<User>> SearchByName(string name);
		Task<List<User>> SearchByRole(string role);
		Task EditUser(User targetUser, UserEdit userEdit, bool removeAvatar, bool removePhoto, byte[] avatarFile, byte[] photoFile, string ip, User user);
		Task EditUserProfileImages(User user, bool removeAvatar, bool removePhoto, byte[] avatarFile, byte[] photoFile);
		Task<UserEdit> GetUserEdit(User user);
		Task EditUserProfile(User user, UserEditProfile userEditProfile);
		bool IsPasswordValid(string password, out string errorMessage);
        Task<bool> IsEmailInUseByDifferentUser(User user, string email);
		Task<List<User>> GetUsersOnline();
		Task<bool> IsIPBanned(string ip);
		Task<bool> IsEmailBanned(string email);
		Task GeneratePasswordResetEmail(User user, string resetLink);
		Task ResetPassword(User user, string newPassword, string ip);
		Task<List<User>> GetUsersFromIDs(IList<int> ids);
		Task<int> GetTotalUsers();
		Task<List<User>> GetSubscribedUsers();
		Dictionary<User, int> GetUsersByPointTotals(int top);
	}

	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;
		private readonly IRoleRepository _roleRepository;
		private readonly IProfileRepository _profileRepository;
		private readonly ISettingsManager _settingsManager;
		private readonly IUserAvatarRepository _userAvatarRepository;
		private readonly IUserImageRepository _userImageRepository;
		private readonly ISecurityLogService _securityLogService;
		private readonly ITextParsingService _textParsingService;
		private readonly IBanRepository _banRepository;
		private readonly IForgotPasswordMailer _forgotPasswordMailer;
		private readonly IImageService _imageService;

		// TODO: Dependencies on imageservice
		public UserService(IUserRepository userRepository, IRoleRepository roleRepository, IProfileRepository profileRepository, ISettingsManager settingsManager, IUserAvatarRepository userAvatarRepository, IUserImageRepository userImageRepository, ISecurityLogService securityLogService, ITextParsingService textParsingService, IBanRepository banRepository, IForgotPasswordMailer forgotPasswordMailer, IImageService imageService
			)
		{
			_userRepository = userRepository;
			_roleRepository = roleRepository;
			_profileRepository = profileRepository;
			_settingsManager = settingsManager;
			_userAvatarRepository = userAvatarRepository;
			_userImageRepository = userImageRepository;
			_securityLogService = securityLogService;
			_textParsingService = textParsingService;
			_banRepository = banRepository;
			_forgotPasswordMailer = forgotPasswordMailer;
			_imageService = imageService;
		}

		public async Task SetPassword(User targetUser, string password, string ip, User user)
		{
			var salt = Guid.NewGuid();
			var hashedPassword = password.GetSHA256Hash(salt);
			await _userRepository.SetHashedPassword(targetUser, hashedPassword, salt);
			await _securityLogService.CreateLogEntry(user, targetUser, ip, string.Empty, SecurityLogType.PasswordChange);
		}

		public async Task<Tuple<bool, Guid?>> CheckPassword(string email, string password)
		{
			string hashedPassword;
			var (storedHash, salt) = await _userRepository.GetHashedPasswordByEmail(email);
			if (salt.HasValue)
				hashedPassword = password.GetSHA256Hash(salt.Value);
			else
				hashedPassword = password.GetSHA256Hash();
			if (storedHash == hashedPassword)
				return Tuple.Create(true, salt);
			// legacy check
			var oldResult = await CheckOldHashedPassword(email, password, salt, storedHash);
			return Tuple.Create(oldResult, salt);
		}

		/// <summary>
		/// This method is used to maintain compatibility with really old and crusty instances of POP Forums
		/// that used MD5 to hash passwords. It upgrades those passwords if they match.
		/// </summary>
		private async Task<bool> CheckOldHashedPassword(string email, string password, Guid? salt, string storedHash)
		{
			string hashedPassword;
			if (salt.HasValue)
				hashedPassword = password.GetMD5Hash(salt.Value);
			else
				hashedPassword = password.GetMD5Hash();
			if (storedHash == hashedPassword)
			{
				// upgrade the password hash
				var user = await _userRepository.GetUserByEmail(email);
				await SetPassword(user, password, string.Empty, null);
				return true;
			}
			return false;
		}

		public async Task<User> GetUser(int userID)
		{
			var user = await _userRepository.GetUser(userID);
			await PopulateRoles(user);
			return user;
		}

		public async Task<User> GetUserByName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return null;
			var user = await _userRepository.GetUserByName(name);
			if (user == null)
				return null;
			await PopulateRoles(user);
			return user;
		}

		public async Task<User> GetUserByAuhtorizationKey(Guid authorizationKey)
		{
			var user = await _userRepository.GetUserByAuthorizationKey(authorizationKey);
			await PopulateRoles(user);
			return user;
		}

		public async Task<User> GetUserByEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return null;
			var user = await _userRepository.GetUserByEmail(email);
			await PopulateRoles(user);
			return user;
		}

		public async Task<List<User>> GetUsersFromIDs(IList<int> ids)
		{
			return await _userRepository.GetUsersFromIDs(ids);
		}

		private async Task PopulateRoles(User user)
		{
			if (user != null)
				user.Roles = await _roleRepository.GetUserRoles(user.UserID);
		}

		public async Task<bool> IsNameInUse(string name)
		{
			return await GetUserByName(name) != null;
		}

		public async Task<bool> IsEmailInUse(string email)
		{
			return await GetUserByEmail(email) != null;
		}

		public async Task<bool> IsEmailInUseByDifferentUser(User user, string email)
		{
			var otherUser = await GetUserByEmail(email);
			if (otherUser == null)
				return false;
			return otherUser.Email != user.Email;
		}

		public async Task<bool> IsIPBanned(string ip)
		{
			return await _banRepository.IPIsBanned(ip);
		}

		public async Task<bool> IsEmailBanned(string email)
		{
			return await _banRepository.EmailIsBanned(email);
		}

		public async Task<User> CreateUser(SignupData signupData, string ip)
		{
			return await CreateUser(signupData.Name, signupData.Email, signupData.Password, _settingsManager.Current.IsNewUserApproved, ip);
		}

		public async Task<User> CreateUser(string name, string email, string password, bool isApproved, string ip)
		{
			name = _textParsingService.Censor(name);
			if (!email.IsEmailAddress())
				throw new Exception("E-mail address invalid.");
			if (string.IsNullOrEmpty(name))
				throw new Exception("Name must not be empty or null.");
			if (await IsNameInUse(name))
				throw new Exception($"The name \"{name}\" is already in use.");
			if (await IsEmailInUse(email))
				throw new Exception($"The e-mail \"{email}\" is already in use.");
			if (await IsIPBanned(ip))
				throw new Exception($"The IP {ip} is banned.");
			if (await IsEmailBanned(email))
				throw new Exception($"The e-mail {email} is banned.");
			var creationDate = DateTime.UtcNow;
			var authorizationKey = Guid.NewGuid();
			var salt = Guid.NewGuid();
			var hashedPassword = password.GetSHA256Hash(salt);
			var user = await _userRepository.CreateUser(name, email, creationDate, isApproved, hashedPassword, authorizationKey, salt);
			await _securityLogService.CreateLogEntry(null, user, ip, string.Empty, SecurityLogType.UserCreated);
			return user;
		}

		public async Task DeleteUser(User targetUser, User user, string ip, bool ban)
		{
			if (ban)
				await _banRepository.BanEmail(targetUser.Email);
			await _userRepository.DeleteUser(targetUser);
			await _securityLogService.CreateLogEntry(user, targetUser, ip, $"Name: {targetUser.Name}, E-mail: {targetUser.Email}", SecurityLogType.UserDeleted);
		}

		public async Task UpdateLastActivityDate(User user)
		{
			await _userRepository.UpdateLastActivityDate(user, DateTime.UtcNow);
		}

		public async Task ChangeEmail(User targetUser, string newEmail, User user, string ip)
		{
			await ChangeEmail(targetUser, newEmail, user, ip, _settingsManager.Current.IsNewUserApproved);
		}

		public async Task ChangeEmail(User targetUser, string newEmail, User user, string ip, bool isUserApproved)
		{
			if (!newEmail.IsEmailAddress())
				throw new Exception("E-mail address invalid.");
			if (await IsEmailInUse(newEmail))
				throw new Exception($"The e-mail \"{newEmail}\" is already in use.");
			var oldEmail = targetUser.Email;
			await _userRepository.ChangeEmail(targetUser, newEmail);
			targetUser.Email = newEmail;
			await _userRepository.UpdateIsApproved(targetUser, isUserApproved);
			await _securityLogService.CreateLogEntry(user, targetUser, ip, $"Old: {oldEmail}, New: {newEmail}", SecurityLogType.EmailChange);
		}

		public async Task ChangeName(User targetUser, string newName, User user, string ip)
		{
			if (string.IsNullOrEmpty(newName))
				throw new Exception("Name must not be empty or null.");
			if (await IsNameInUse(newName))
				throw new Exception($"The name \"{newName}\" is already in use.");
			var oldName = targetUser.Name;
			await _userRepository.ChangeName(targetUser, newName);
			targetUser.Name = newName;
			await _securityLogService.CreateLogEntry(user, targetUser, ip, $"Old: {oldName}, New: {newName}", SecurityLogType.NameChange);
		}

		public async Task UpdateIsApproved(User targetUser, bool isApproved, User user, string ip)
		{
			if (targetUser == null)
				throw new ArgumentNullException("targetUser");
			await _userRepository.UpdateIsApproved(targetUser, isApproved);
			var logType = isApproved ? SecurityLogType.IsApproved : SecurityLogType.IsNotApproved;
			await _securityLogService.CreateLogEntry(user, targetUser, ip, String.Empty, logType);
		}

		public async Task UpdateAuthorizationKey(User user, Guid key)
		{
			if (user == null)
				throw new ArgumentNullException("user");
			await _userRepository.UpdateAuthorizationKey(user, key);
		}

		public async Task Logout(User user, string ip)
		{
			// used only for logging; controller performs actual logout
			await _securityLogService.CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.Logout);
		}

		public async Task<Tuple<bool, User>> Login(string email, string password, string ip)
		{
			User user;
			var (result, salt) = await CheckPassword(email, password);
			if (result)
			{
				user = await GetUserByEmail(email);
				await _userRepository.UpdateLastLoginDate(user, DateTime.UtcNow);
				await _securityLogService.CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.Login);
				if (!salt.HasValue)
					await SetPassword(user, password, ip, user);
			}
			else
			{
				user = null;
				await _securityLogService.CreateLogEntry((User)null, null, ip, "E-mail attempted: " + email, SecurityLogType.FailedLogin);
			}
			return Tuple.Create(result, user);
		}

		public async Task Login(User user, string ip)
		{
			await _userRepository.UpdateLastLoginDate(user, DateTime.UtcNow);
			await _securityLogService.CreateLogEntry(null, user, ip, String.Empty, SecurityLogType.Login);
		}

		public async Task<List<string>> GetAllRoles()
		{
			return await _roleRepository.GetAllRoles();
		}

		public async Task CreateRole(string role, User user, string ip)
		{
			await _roleRepository.CreateRole(role);
			await _securityLogService.CreateLogEntry(user, null, ip, "Role: " + role, SecurityLogType.RoleCreated);
		}

		public async Task DeleteRole(string role, User user, string ip)
		{
			if (role.ToLower() == PermanentRoles.Admin.ToLower() || role.ToLower() == PermanentRoles.Moderator.ToLower())
				throw new InvalidOperationException("Can't delete Admin or Moderator roles.");
			await _roleRepository.DeleteRole(role);
			await _securityLogService.CreateLogEntry(user, null, ip, "Role: " + role, SecurityLogType.RoleDeleted);
		}

		public async Task<User> VerifyAuthorizationCode(Guid key, string ip)
		{
			var targetUser = await _userRepository.GetUserByAuthorizationKey(key);
			if (targetUser == null)
				return null;
			var newKey = Guid.NewGuid();
			await UpdateAuthorizationKey(targetUser, newKey);
			await UpdateIsApproved(targetUser, true, null, ip);
			targetUser.AuthorizationKey = newKey;
			return targetUser;
		}

		public async Task<List<User>> SearchByEmail(string email)
		{
			return await _userRepository.SearchByEmail(email);
		}

		public async Task<List<User>> SearchByName(string name)
		{
			return await _userRepository.SearchByName(name);
		}

		public async Task<List<User>> SearchByRole(string role)
		{
			return await _userRepository.SearchByRole(role);
		}

		public async Task<UserEdit> GetUserEdit(User user)
		{
			if (user == null)
				throw new ArgumentNullException("user");
			var profile = await _profileRepository.GetProfile(user.UserID);
			return new UserEdit(user, profile);
		}

		public async Task EditUser(User targetUser, UserEdit userEdit, bool removeAvatar, bool removePhoto, byte[] avatarFile, byte[] photoFile, string ip, User user)
		{
			if (!string.IsNullOrWhiteSpace(userEdit.NewEmail))
				await ChangeEmail(targetUser, userEdit.NewEmail, user, ip, userEdit.IsApproved);
			if (!string.IsNullOrWhiteSpace(userEdit.NewPassword))
				await SetPassword(targetUser, userEdit.NewPassword, ip, user);
			if (targetUser.IsApproved != userEdit.IsApproved)
				await UpdateIsApproved(targetUser, userEdit.IsApproved, user, ip);

			var profile = await _profileRepository.GetProfile(targetUser.UserID);
			profile.IsSubscribed = userEdit.IsSubscribed;
			profile.ShowDetails = userEdit.ShowDetails;
			profile.IsPlainText = userEdit.IsPlainText;
			profile.HideVanity = userEdit.HideVanity;
			profile.TimeZone = userEdit.TimeZone;
			profile.IsDaylightSaving = userEdit.IsDaylightSaving;
			profile.Signature = _textParsingService.ForumCodeToHtml(userEdit.Signature);
			profile.Location = userEdit.Location;
			profile.Dob = userEdit.Dob;
			profile.Web = userEdit.Web;
			profile.Instagram = userEdit.Instagram;
			profile.Facebook = userEdit.Facebook;
			profile.Twitter = userEdit.Twitter;
			if (removeAvatar)
				profile.AvatarID = null;
			if (removePhoto)
				profile.ImageID = null;
			await _profileRepository.Update(profile);

			var newRoles = userEdit.Roles ?? new string[0];
			await _roleRepository.ReplaceUserRoles(targetUser.UserID, newRoles);
			foreach (var role in targetUser.Roles)
				if (!newRoles.Contains(role))
					await _securityLogService.CreateLogEntry(user, targetUser, ip, role, SecurityLogType.UserRemovedFromRole);
			foreach (var role in newRoles)
				if (!targetUser.Roles.Contains(role))
					await _securityLogService.CreateLogEntry(user, targetUser, ip, role, SecurityLogType.UserAddedToRole);

			if (avatarFile != null && avatarFile.Length > 0)
			{
				var avatarID = await _userAvatarRepository.SaveNewAvatar(targetUser.UserID, avatarFile, DateTime.UtcNow);
				profile.AvatarID = avatarID;
				await _profileRepository.Update(profile);
			}

			if (photoFile != null && photoFile.Length > 0)
			{
				var imageID = await _userImageRepository.SaveNewImage(targetUser.UserID, 0, true, photoFile, DateTime.UtcNow);
				profile.ImageID = imageID;
				await _profileRepository.Update(profile);
			}
		}

		public async Task EditUserProfileImages(User user, bool removeAvatar, bool removePhoto, byte[] avatarFile, byte[] photoFile)
		{
			var profile = await _profileRepository.GetProfile(user.UserID);
			if (removeAvatar)
			{
				await _userAvatarRepository.DeleteAvatarsByUserID(user.UserID);
				profile.AvatarID = null;
			}
			if (removePhoto)
			{
				await _userImageRepository.DeleteImagesByUserID(user.UserID);
				profile.ImageID = null;
			}
			await _profileRepository.Update(profile);

			if (avatarFile != null && avatarFile.Length > 0)
			{
				await _userAvatarRepository.DeleteAvatarsByUserID(user.UserID);
				var bytes = _imageService.ConstrainResize(avatarFile, _settingsManager.Current.UserAvatarMaxWidth, _settingsManager.Current.UserAvatarMaxHeight, 70);
				var avatarID = await _userAvatarRepository.SaveNewAvatar(user.UserID, bytes, DateTime.UtcNow);
				profile.AvatarID = avatarID;
				await _profileRepository.Update(profile);
			}

			if (photoFile != null && photoFile.Length > 0)
			{
				await _userImageRepository.DeleteImagesByUserID(user.UserID);
				var bytes = _imageService.ConstrainResize(photoFile, _settingsManager.Current.UserImageMaxWidth, _settingsManager.Current.UserImageMaxHeight, 70);
				var imageID = await _userImageRepository.SaveNewImage(user.UserID, 0, _settingsManager.Current.IsNewUserImageApproved, bytes, DateTime.UtcNow);
				profile.ImageID = imageID;
				await _profileRepository.Update(profile);
			}
		}

		// TODO: this and some other stuff probably belongs in ProfileService
		public async Task EditUserProfile(User user, UserEditProfile userEditProfile)
		{
			var profile = await _profileRepository.GetProfile(user.UserID);
			if (profile == null)
				throw new Exception($"No profile found for UserID {user.UserID}");
			profile.IsSubscribed = userEditProfile.IsSubscribed;
			profile.ShowDetails = userEditProfile.ShowDetails;
			profile.IsPlainText = userEditProfile.IsPlainText;
			profile.HideVanity = userEditProfile.HideVanity;
			profile.TimeZone = userEditProfile.TimeZone;
			profile.IsDaylightSaving = userEditProfile.IsDaylightSaving;
			profile.Signature = _textParsingService.ForumCodeToHtml(userEditProfile.Signature);
			profile.Location = userEditProfile.Location;
			profile.Dob = userEditProfile.Dob;
			profile.Web = userEditProfile.Web;
			profile.Instagram = userEditProfile.Instagram;
			profile.Facebook = userEditProfile.Facebook;
			profile.Twitter = userEditProfile.Twitter;
			await _profileRepository.Update(profile);
		}

		public bool IsPasswordValid(string password, out string errorMessage)
		{
			if (String.IsNullOrEmpty(password) || password.Length < 6)
			{
				errorMessage = "Password must be at least six characters";
				return false;
			}
			errorMessage = null;
			return true;
		}

		public async Task<List<User>> GetUsersOnline()
		{
			return await _userRepository.GetUsersOnline();
		}

		public async Task<int> GetTotalUsers()
		{
			return await _userRepository.GetTotalUsers();
		}

		public async Task GeneratePasswordResetEmail(User user, string resetLink)
		{
			if (user == null)
				throw new ArgumentNullException("user");
			var newAuth = Guid.NewGuid();
			await UpdateAuthorizationKey(user, newAuth);
			user.AuthorizationKey = newAuth;
			var link = resetLink + "/" + newAuth;
			await _forgotPasswordMailer.ComposeAndQueue(user, link);
		}

		public async Task ResetPassword(User user, string newPassword, string ip)
		{
			await SetPassword(user, newPassword, ip, null);
			await UpdateAuthorizationKey(user, Guid.NewGuid());
			await Login(user, ip);
		}

		public async Task<List<User>> GetSubscribedUsers()
		{
			return await _userRepository.GetSubscribedUsers();
		}

		public Dictionary<User, int> GetUsersByPointTotals(int top)
		{
			return _userRepository.GetUsersByPointTotals(top);
		}
	}
}