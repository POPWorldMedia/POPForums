using System;
using System.Collections.Generic;
using NUnit.Framework;
using PopForums.Models;

namespace PopForums.Test.Models
{
	[TestFixture]
	public class UserTest
	{
		[Test]
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
			Assert.AreEqual(userID, user.UserID);
			Assert.AreEqual(name, user.Name);
			Assert.AreEqual(email, user.Email);
			Assert.AreEqual(createDate, user.CreationDate);
			Assert.AreEqual(approved, user.IsApproved);
			Assert.AreEqual(lastActivity, user.LastActivityDate);
			Assert.AreEqual(lastLogin, user.LastLoginDate);
			Assert.AreEqual(authKey, user.AuthorizationKey);
		}

		[Test]
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