namespace PopForums.Test.Services;

public class ClientSettingsMapperTests
{
	[Fact]
	public void MapPlainText()
	{
		var profile = new Profile
		{
			IsPlainText = true,
			HideVanity = false
		};
		var mapper = new ClientSettingsMapper();
		var settings = mapper.GetClientSettings(profile);
		Assert.Equal(profile.IsPlainText, settings.UsePlainText);
	}

	[Fact]
	public void MapHideVanity()
	{
		var profile = new Profile
		{
			IsPlainText = false,
			HideVanity = true
		};
		var mapper = new ClientSettingsMapper();
		var settings = mapper.GetClientSettings(profile);
		Assert.Equal(profile.HideVanity, settings.HideVanity);
	}
}