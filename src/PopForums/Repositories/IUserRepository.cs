using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
		Task SetHashedPassword(User user, string hashedPassword, Guid salt);

		/// <summary>
		/// Gets the hashed password and salt from the data store of the user whose e-mail is matched.
		/// </summary>
		/// <param name="email"></param>
		/// <returns>A tuple containing the hashed password and salt.</returns>
		Task<Tuple<string, Guid?>> GetHashedPasswordByEmail(string email);

		/// <summary>
		/// Gets a user by its ID.
		/// </summary>
		/// <param name="userID">UserID to match.</param>
		/// <returns>Matched user, or null if no matching user is found.</returns>
		Task<User> GetUser(int userID);

		/// <summary>
		/// Gets a user by name. Match must be case insensitive.
		/// </summary>
		/// <param name="name">Name to match.</param>
		/// <returns>Matched user, or null if no matching user is found.</returns>
		Task<User> GetUserByName(string name);

		/// <summary>
		/// Gets a user by e-mail address. Match must be case insensitive.
		/// </summary>
		/// <param name="email">E-mail to match.</param>
		/// <returns>Matched user, or null if no matching user is found.</returns>
		Task<User> GetUserByEmail(string email);

		Task<User> GetUserByAuthorizationKey(Guid key);

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
		Task<User> CreateUser(string name, string email, DateTime creationDate, bool isApproved, string hashedPassword, Guid authorizationKey, Guid salt);

		/// <summary>
		/// Updates a user record in the data store with a new LastActivityDate.
		/// </summary>
		/// <param name="user">User to update.</param>
		/// <param name="newDate">New value for LastActivityDate value.</param>
		Task UpdateLastActivityDate(User user, DateTime newDate);

		/// <summary>
		/// Updates a user record in the data store with a new LastLoginDate.
		/// </summary>
		/// <param name="user">User to update.</param>
		/// <param name="newDate">New value for LastActivityDate value.</param>
		Task UpdateLastLoginDate(User user, DateTime newDate);

		/// <summary>
		/// Updates a user record with a new name.
		/// </summary>
		/// <param name="user">User to update.</param>
		/// <param name="newName">New name. Must not be in use by another user, null or empty.</param>
		Task ChangeName(User user, string newName);

		/// <summary>
		/// Updates a user record with a new e-mail address.
		/// </summary>
		/// <param name="user">User to update.</param>
		/// <param name="newEmail">New e-mail address. Must not be in use by another user.</param>
		Task ChangeEmail(User user, string newEmail);

		Task UpdateIsApproved(User user, bool isApproved);
		Task UpdateAuthorizationKey(User user, Guid key);
		Task<List<User>> SearchByEmail(string email);
		Task<List<User>> SearchByName(string name);
		Task<List<User>> SearchByRole(string role);
		Task<List<User>> GetUsersOnline();
		Task<List<User>> GetSubscribedUsers();
		Task DeleteUser(User user);
		Task<List<User>> GetUsersFromIDs(IList<int> ids);
		Task<int> GetTotalUsers();
		Dictionary<User, int> GetUsersByPointTotals(int top);
	}
}
