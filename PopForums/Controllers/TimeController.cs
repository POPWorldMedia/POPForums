using System;
using System.Collections.Generic;
using System.Web.Mvc;
using PopForums.Configuration.DependencyResolution;
using PopForums.Extensions;
using PopForums.Services;

namespace PopForums.Controllers
{
	public class TimeController : Controller
	{
		public TimeController()
		{
			var serviceLocator = StructuremapMvc.StructureMapDependencyScope;
			_timeFormattingService = serviceLocator.GetInstance<ITimeFormattingService>();
			_profileService = serviceLocator.GetInstance<IProfileService>();
		}

		protected internal TimeController(ITimeFormattingService timeFormattingService, IProfileService profileService)
		{
			_timeFormattingService = timeFormattingService;
			_profileService = profileService;
		}

		public static string Name = "Time";

		private readonly ITimeFormattingService _timeFormattingService;
		private readonly IProfileService _profileService;

		[HttpPost]
		public JsonResult GetTimes(string[] times)
		{
			var list = new List<TimePairs>();
			if (times == null || times.Length == 0)
				return Json(list);
			var user = this.CurrentUser();
			var profile = _profileService.GetProfile(user);
			_timeFormattingService.Init(profile);
			foreach (var item in times)
			{
				var time = DateTime.Parse(item);
				list.Add(new TimePairs {Key = item, Value = _timeFormattingService.GetFormattedTime(time)});
			}
			return Json(list);
		}

		private class TimePairs
		{
			public string Key { get; set; }
			public string Value { get; set; }
		}
	}
}