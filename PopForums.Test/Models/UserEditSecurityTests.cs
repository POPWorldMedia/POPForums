using NUnit.Framework;
using PopForums.Models;

namespace PopForums.Test.Models
{
	[TestFixture]
	public class UserEditSecurityTests
	{
		[Test]
		public void PasswordsMatch()
		{
			var edit = new UserEditSecurity();
			edit.NewPassword = "blah";
			edit.NewPasswordRetype = "blah";
			Assert.IsTrue(edit.NewPasswordsMatch());
		}

		[Test]
		public void PasswordsNoMatch()
		{
			var edit = new UserEditSecurity();
			edit.NewPassword = "blasjspvjsh";
			edit.NewPasswordRetype = "blah";
			Assert.IsFalse(edit.NewPasswordsMatch());
		}

		[Test]
		public void EmailMatch()
		{
			var edit = new UserEditSecurity();
			edit.NewEmail = "blah";
			edit.NewEmailRetype = "blah";
			Assert.IsTrue(edit.NewEmailsMatch());
		}

		[Test]
		public void EmailNoMatch()
		{
			var edit = new UserEditSecurity();
			edit.NewEmail = "blah";
			edit.NewEmailRetype = "bloidsvosah";
			Assert.IsFalse(edit.NewEmailsMatch());
		}
	}
}
