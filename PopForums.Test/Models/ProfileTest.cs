using NUnit.Framework;
using PopForums.Models;

namespace PopForums.Test.Models
{
	[TestFixture]
	public class ProfileTest
	{
		[Test]
		public void UserIDSet()
		{
			const int id = 123;
			var profile = new Profile(id);
			Assert.AreEqual(id, profile.UserID);
		}
	}
}