using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using PopForums.Models;

namespace PopForums.Services
{
	public interface IUserService
	{
		void SetPassword(User targetUser, string password, string ip, User user);
		bool CheckPassword(string email, string password, out Guid? salt);
		User GetUser(int userID);
		User GetUserByName(string name);
		User GetUserByEmail(string email);
		User GetUserByAuhtorizationKey(Guid authorizationKey);
		bool IsNameInUse(string name);
		bool IsEmailInUse(string email);
		User CreateUser(SignupData signupData, string ip);
		User CreateUser(string name, string email, string password, bool isApproved, string ip);
		void DeleteUser(User targetUser, User user, string ip, bool ban);
		void UpdateLastActicityDate(User user);
		void ChangeEmail(User targetUser, string newEmail, User user, string ip);
		void ChangeName(User targetUser, string newName, User user, string ip);
		void UpdateIsApproved(User targetUser, bool isApproved, User user, string ip);
		void UpdateAuthorizationKey(User user, Guid key);
		User SetupUserViewData(IPrincipal principal, ViewDataDictionary viewData);
		void Logout(User user, string ip);
		bool Login(string email, string password, bool persistCookie, HttpContextBase context);
		void Login(User user, HttpContextBase context);
		void Login(User user, bool persistCookie, HttpContextBase context);
		List<string> GetAllRoles();
		void CreateRole(string role, User user, string ip);
		void DeleteRole(string role, User user, string ip);
		User VerifyAuthorizationCode(Guid key, string ip);
		List<User> SearchByEmail(string email);
		List<User> SearchByName(string name);
		List<User> SearchByRole(string role);
		void EditUser(User targetUser, UserEdit userEdit, bool removeAvatar, bool removePhoto, HttpPostedFileBase avatarFile, HttpPostedFileBase photoFile, string ip, User user);
		void EditUserProfileImages(User user, bool removeAvatar, bool removePhoto, HttpPostedFileBase avatarFile, HttpPostedFileBase photoFile);
		UserEdit GetUserEdit(User user);
		void EditUserProfile(User user, UserEditProfile userEditProfile);
		bool VerifyPassword(User user, string password);
		bool IsPasswordValid(string password, ModelStateDictionary modelStateDictionary);
		bool IsEmailInUseByDifferentUser(User user, string email);
		List<User> GetUsersOnline();
		bool IsIPBanned(string ip);
		bool IsEmailBanned(string email);
		void GeneratePasswordResetEmail(User user, string resetLink);
		void ResetPassword(User user, string newPassword, HttpContextBase httpContext);
		List<User> GetUsersFromIDs(IList<int> ids);
		int GetTotalUsers();
		List<User> GetSubscribedUsers();
		Dictionary<User, int> GetUsersByPointTotals(int top);
	}
}