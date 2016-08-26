using PopForums.Models;
using Xunit;

namespace PopForums.Test.Models
{
	public class ProfileTest
	{
		[Fact]
		public void UserIDSet()
		{
			const int id = 123;
			var profile = new Profile(id);
			Assert.Equal(id, profile.UserID);
		}
	}
}