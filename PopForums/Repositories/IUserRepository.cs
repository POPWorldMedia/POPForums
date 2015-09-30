using System;
using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Repositories
{
	public interface IUserRepository
	{
		/// <summary>
		/// Stores the hashed password in the data store.
		/// </summary>
		/// <param name="user">User to update with new hashed password.</param>
		/// <param name="hashedPassword">The string of the hashed password.</param>
		/// <param name="salt">A Guid that was used in hashing the password.</param>
		void SetHashedPassword(User user, string hashedPassword, Guid salt);

		/// <summary>
		/// Gets the hashed password from the data store of the user whose e-mail is matched.
		/// </summary>
		/// <param name="email">E-mail of user to match.</param>
		/// <param name="salt">An output param that was used in salting the hash.</param>
		/// <returns>Hashed password, or null if no match is found.</returns>
		string GetHashedPasswordByEmail(string email, out Guid? salt);

		/// <summary>
		/// Gets a user by its ID.
		/// </summary>
		/// <param name="userID">UserID to match.</param>
		/// <returns>Matched user, or null if no matching user is found.</returns>
		User GetUser(int userID);

		/// <summary>
		/// Gets a user by name. Match must be case insensitive.
		/// </summary>
		/// <param name="name">Name to match.</param>
		/// <returns>Matched user, or null if no matching user is found.</returns>
		User GetUserByName(string name);

		/// <summary>
		/// Gets a user by e-mail address. Match must be case insensitive.
		/// </summary>
		/// <param name="email">E-mail to match.</param>
		/// <returns>Matched user, or null if no matching user is found.</returns>
		User GetUserByEmail(string email);

		User GetUserByAuthorizationKey(Guid key);

		/// <summary>
		/// Stores new user data and returns a new User object, with new UserID.
		/// </summary>
		/// <param name="name">Name for new user. Must not be in use.</param>
		/// <param name="email">E-mail address for new user. Must not be in use.</param>
		/// <param name="creationDate">Date (UTC) to assign as creation date. LastActivityDate and LastLoginDate will be set to this as well.</param>
		/// <param name="isApproved">Boolean indicating user approval.</param>
		/// <param name="hashedPassword">The hashed password string.</param>
		/// <param name="authorizationKey">A Guid used for authorization measures.</param>
		/// <param name="salt">A Guid used to salt the password.</param>
		/// <returns>A new User object, populated with the data store generated UserID.</returns>
		User CreateUser(string name, string email, DateTime creationDate, bool isApproved, string hashedPassword, Guid authorizationKey, Guid salt);

		/// <summary>
		/// Updates a user record in the data store with a new LastActivityDate.
		/// </summary>
		/// <param name="user">User to update.</param>
		/// <param name="newDate">New value for LastActivityDate value.</param>
		void UpdateLastActivityDate(User user, DateTime newDate);

		/// <summary>
		/// Updates a user record in the data store with a new LastLoginDate.
		/// </summary>
		/// <param name="user">User to update.</param>
		/// <param name="newDate">New value for LastActivityDate value.</param>
		void UpdateLastLoginDate(User user, DateTime newDate);

		/// <summary>
		/// Updates a user record with a new name.
		/// </summary>
		/// <param name="user">User to update.</param>
		/// <param name="newName">New name. Must not be in use by another user, null or empty.</param>
		void ChangeName(User user, string newName);

		/// <summary>
		/// Updates a user record with a new e-mail address.
		/// </summary>
		/// <param name="user">User to update.</param>
		/// <param name="newEmail">New e-mail address. Must not be in use by another user.</param>
		void ChangeEmail(User user, string newEmail);

		void UpdateIsApproved(User user, bool isApproved);
		void UpdateAuthorizationKey(User user, Guid key);
		List<User> SearchByEmail(string email);
		List<User> SearchByName(string name);
		List<User> SearchByRole(string role);
		List<User> GetUsersOnline();
		List<User> GetSubscribedUsers();
		void DeleteUser(User user);
		List<User> GetUsersFromIDs(IList<int> ids);
		int GetTotalUsers();
		Dictionary<User, int> GetUsersByPointTotals(int top);
	}
}
