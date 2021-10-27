namespace PopForums.Services;

public interface ITimeFormattingService
{
	string GetFormattedTime(DateTime utcDateTime, Profile profile);
}

public class TimeFormattingService : ITimeFormattingService
{
	public TimeFormattingService(ISettingsManager settingsManager)
	{
		if (settingsManager == null)
			throw new ArgumentNullException(nameof(settingsManager));
		_settingsManager = settingsManager;
	}

	private readonly ISettingsManager _settingsManager;
	private int _utcOffset;
	private bool _usesDaylightSaving;

	public string GetFormattedTime(DateTime utcDateTime, Profile profile)
	{
		_utcOffset = _settingsManager.Current.ServerTimeZone;
		_usesDaylightSaving = _settingsManager.Current.ServerDaylightSaving;
		if (profile != null)
		{
			_utcOffset = profile.TimeZone;
			_usesDaylightSaving = profile.IsDaylightSaving;
		}
		var now = GetAdjustedTime(DateTime.UtcNow);
		var input = GetAdjustedTime(utcDateTime);
		var difference = now.Subtract(input);
		if (difference > new TimeSpan(0, 59, 59))
		{
			if (now.Date == input.Date)
				return String.Format(Resources.TodayTime, input.ToString("t"));
			if (now.Date.AddDays(-1) == input.Date)
				return String.Format(Resources.YesterdayTime, input.ToString("t"));
			return input.ToString("f");
		}
		if (difference > new TimeSpan(0, 2, 00))
			return String.Format(Resources.MinutesAgo, difference.Minutes);
		if (difference > new TimeSpan(0, 1, 00))
			return Resources.OneMinuteAgo;
		return Resources.LessThanMinute;
	}

	private DateTime GetAdjustedTime(DateTime dateTime)
	{
		double offset = _utcOffset;
		if (_usesDaylightSaving && IsDaylightSaving(dateTime)) offset++;
		return dateTime.AddHours(offset);
	}

	private bool IsDaylightSaving(DateTime localTime)
	{
		DateTime startDate;
		DateTime endDate;
		if ((_utcOffset < -4) && (_utcOffset > -11))
		{
			// us dst
			if (localTime.Year < 2007)
			{
				startDate = new DateTime(localTime.Year, 4, Convert.ToInt32((2 + 6 * localTime.Year - Math.Floor((double)localTime.Year / 4)) % 7) + 1, 2, 0, 0);
				endDate = new DateTime(localTime.Year, 10, Convert.ToInt32(31 - (Math.Floor((double)localTime.Year * 5 / 4) + 1) % 7), 2, 0, 0);
			}
			else
			{
				startDate = new DateTime(localTime.Year, 3, 14 - Convert.ToInt32(Math.Floor(1 + (double)localTime.Year * 5 / 4) % 7), 2, 0, 0);
				endDate = new DateTime(localTime.Year, 11, 7 - Convert.ToInt32(Math.Floor(1 + (double)localTime.Year * 5 / 4) % 7), 2, 0, 0);
			}
			if ((localTime > startDate) && (localTime < endDate)) return true;
		}
		if ((_utcOffset > -1) && (_utcOffset < 5))
		{
			// european dst
			startDate = new DateTime(localTime.Year, 3, Convert.ToInt32(31 - (Math.Floor((double)localTime.Year * 5 / 4) + 1) % 7), 1, 0, 0);
			endDate = new DateTime(localTime.Year, 10, Convert.ToInt32(31 - (Math.Floor((double)localTime.Year * 5 / 4) + 1) % 7), 1, 0, 0);
			if ((localTime > startDate) && (localTime < endDate)) return true;
		}
		return false;
	}
}