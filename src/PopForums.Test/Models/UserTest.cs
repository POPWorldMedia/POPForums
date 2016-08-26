using System;
using System.Collections.Generic;
using PopForums.Models;
using Xunit;

namespace PopForums.Test.Models
{
	public class UserTest
	{
		[Fact]
		public void CreateUser()
		{
			const int userID = 123;
			const string name = "Jeff";
			const string email = "a@b.com";
			var createDate = DateTime.UtcNow;
			const bool approved = true;
			var lastActivity = DateTime.UtcNow.AddDays(-1);
			var lastLogin = DateTime.UtcNow.AddDays(-2);
			var authKey = Guid.NewGuid();
			var user = new User(userID, createDate) {Name = name, Email = email, IsApproved = approved, LastActivityDate = lastActivity, LastLoginDate = lastLogin, AuthorizationKey = authKey};
			Assert.Equal(userID, user.UserID);
			Assert.Equal(name, user.Name);
			Assert.Equal(email, user.Email);
			Assert.Equal(createDate, user.CreationDate);
			Assert.Equal(approved, user.IsApproved);
			Assert.Equal(lastActivity, user.LastActivityDate);
			Assert.Equal(lastLogin, user.LastLoginDate);
			Assert.Equal(authKey, user.AuthorizationKey);
		}

		[Fact]
		public void IsRoleWiredToRoles()
		{
			var user = GetTestUser();
			user.Roles = new List<string> {"blah", "three", PermanentRoles.Admin};
			Assert.True(user.IsInRole(PermanentRoles.Admin));
		}

		public static User GetTestUser()
		{
			const int userID = 123;
			const string name = "Jeff";
			const string email = "a@b.com";
			var createDate = DateTime.UtcNow;
			const bool approved = true;
			var lastActivity = DateTime.UtcNow.AddDays(-1);
			var lastLogin = DateTime.UtcNow.AddDays(-2);
			var authKey = Guid.NewGuid();
			return new User(userID, createDate) { Name = name, Email = email, IsApproved = approved, LastActivityDate = lastActivity, LastLoginDate = lastLogin, AuthorizationKey = authKey, Roles = new List<string>() };
		}
	}
}