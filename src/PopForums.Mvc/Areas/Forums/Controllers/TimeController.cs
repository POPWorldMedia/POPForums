using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopForums.Mvc.Areas.Forums.Services;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class TimeController : Controller
	{
		public TimeController(ITimeFormattingService timeFormattingService, IProfileService profileService, IUserRetrievalShim userRetrievalShim)
		{
			_timeFormattingService = timeFormattingService;
			_profileService = profileService;
			_userRetrievalShim = userRetrievalShim;
		}

		public static string Name = "Time";

		private readonly ITimeFormattingService _timeFormattingService;
		private readonly IProfileService _profileService;
		private readonly IUserRetrievalShim _userRetrievalShim;

		[HttpPost]
		public async Task<JsonResult> GetTimes(string[] times)
		{
			var list = new List<TimePairs>();
			if (times == null || times.Length == 0)
				return Json(list);
			var user = _userRetrievalShim.GetUser();
			var profile = await _profileService.GetProfile(user);
			foreach (var item in times)
			{
				var time = DateTime.Parse(item);
				list.Add(new TimePairs { Key = item, Value = _timeFormattingService.GetFormattedTime(time, profile) });
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