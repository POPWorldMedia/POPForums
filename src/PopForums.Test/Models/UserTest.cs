using System;
using System.Collections.Generic;
using PopForums.Models;
using Xunit;

namespace PopForums.Test.Models
{
	public class UserTest
	{
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
			var authKey = Guid.NewGuid();
			return new User { UserID = userID, Name = name, Email = email, CreationDate = createDate, IsApproved = approved, AuthorizationKey = authKey, Roles = new List<string>() };
		}
	}
}