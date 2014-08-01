using System;
using System.Web.Mvc;
using PopForums.Models;
using PopForums.Services;

namespace PopForums.Web
{
	public abstract class PopForumsRazorViewPage : PopForumsRazorViewPage<dynamic>
	{
		
	}

	public abstract class PopForumsRazorViewPage<T> : WebViewPage<T>
	{
		protected PopForumsRazorViewPage()
		{
			var container = PopForumsActivation.ServiceLocator;
			TimeFormattingService = container.GetInstance<ITimeFormattingService>();
			ProfileService = container.GetInstance<IProfileService>();
		}

		public ITimeFormattingService TimeFormattingService { get; set; }
		public IProfileService ProfileService { get; set; }

		private readonly object _sync = new Object();

		public Profile ForumProfile
		{
			get
			{
				EnsureProfileLoad();
				return _profile;
			}
		}
		private Profile _profile;

		public string FormatTime(DateTime utcDateTime)
		{
			EnsureProfileLoad();
			return TimeFormattingService.GetFormattedTime(utcDateTime);
		}

		public string FormatTime8601(DateTime dateTime)
		{
			return dateTime.ToString("o");
		}

		public bool HideVanity()
		{
			EnsureProfileLoad();
			if (ForumProfile == null)
				return false;
			return ForumProfile.HideVanity;
		}

		private bool _profileLoadAttempted;
		private void EnsureProfileLoad()
		{
			lock (_sync)
			{
				if (_profileLoadAttempted) return;
				var user = User as User;
				if (user == null)
				{
					TimeFormattingService.Init(null);
					return;
				}
				_profile = ProfileService.GetProfile(user);
				TimeFormattingService.Init(_profile);
				_profileLoadAttempted = true;
			}
		}
	}
}
