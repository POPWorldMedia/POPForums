namespace PopForums.Services;

public interface ITimeFormatStringService
{
	TimeFormats GeTimeFormats();
	string GetTimeFormatsAsJson();
}

public class TimeFormatStringService : ITimeFormatStringService
{
	public TimeFormats GeTimeFormats()
	{
		var formats = new TimeFormats
		{
			TodayTime = Resources.TodayTime,
			YesterdayTime = Resources.YesterdayTime,
			MinutesAgo = Resources.MinutesAgo,
			OneMinuteAgo = Resources.OneMinuteAgo,
			LessThanMinute = Resources.LessThanMinute
		};
		return formats;
	}

	public string GetTimeFormatsAsJson()
	{
		var formats = GeTimeFormats();
		var serialized = JsonSerializer.Serialize(formats, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
		return serialized;
	}
}