using NUnit.Framework;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.Test.Services
{
	[TestFixture]
	public class ClientSettingsMapperTests
	{
		[Test]
		public void MapPlainText()
		{
			var profile = new Profile
			              	{
			              		IsPlainText = true,
			              		HideVanity = false
			              	};
			var mapper = new ClientSettingsMapper();
			var settings = mapper.GetClientSettings(profile);
			Assert.AreEqual(profile.IsPlainText, settings.UsePlainText);
		}

		[Test]
		public void MapHideVanity()
		{
			var profile = new Profile
			{
				IsPlainText = false,
				HideVanity = true
			};
			var mapper = new ClientSettingsMapper();
			var settings = mapper.GetClientSettings(profile);
			Assert.AreEqual(profile.HideVanity, settings.HideVanity);
		}
	}
}