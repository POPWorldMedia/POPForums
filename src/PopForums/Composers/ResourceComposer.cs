﻿namespace PopForums.Composers;

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

			Notifications = Resources.Notifications,
			NewReplyNotification = Resources.NewReplyNotification,
			Award = Resources.Award,
			VoteUpNotification = Resources.VoteUpNotification,
			QuestionAnsweredNotification = Resources.QuestionAnsweredNotification,
			Send = Resources.Send,

			UploadImage = Resources.UploadImage
		};
		return resources;
	}
}