namespace PopForums.Composers;

public interface IResourceComposer
{
	dynamic GetForCurrentThread();
}

public class ResourceComposer : IResourceComposer
{
	public dynamic GetForCurrentThread()
	{
		var resources = new
		{
			LessThanMinute = Resources.LessThanMinute,
			OneMinuteAgo = Resources.OneMinuteAgo,
			MinutesAgo = Resources.MinutesAgo,
			TodayTime = Resources.TodayTime,
			YesterdayTime = Resources.YesterdayTime,
			Notifications = Resources.Notifications
		};
		return resources;
	}
}