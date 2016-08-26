using System;
using PopForums.Models;
using Xunit;

namespace PopForums.Test.Models
{
	public class UserEditSecurityTests
	{
		[Fact]
		public void PasswordsMatch()
		{
			var edit = new UserEditSecurity();
			edit.NewPassword = "blah";
			edit.NewPasswordRetype = "blah";
			Assert.True(edit.NewPasswordsMatch());
		}

		[Fact]
		public void PasswordsNoMatch()
		{
			var edit = new UserEditSecurity();
			edit.NewPassword = "blasjspvjsh";
			edit.NewPasswordRetype = "blah";
			Assert.False(edit.NewPasswordsMatch());
		}

		[Fact]
		public void EmailMatch()
		{
			var edit = new UserEditSecurity();
			edit.NewEmail = "blah";
			edit.NewEmailRetype = "blah";
			Assert.True(edit.NewEmailsMatch());
		}

		[Fact]
		public void EmailNoMatch()
		{
			var edit = new UserEditSecurity();
			edit.NewEmail = "blah";
			edit.NewEmailRetype = "bloidsvosah";
			Assert.False(edit.NewEmailsMatch());
		}

		[Fact]
		public void IsNewUserApprovedMapped()
		{
			var edit = new UserEditSecurity(new User(1, DateTime.MaxValue), true);
			Assert.True(edit.IsNewUserApproved);
		}
	}
}
