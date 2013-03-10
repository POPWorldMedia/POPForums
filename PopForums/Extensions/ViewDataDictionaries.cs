using System.Web.Mvc;
using PopForums.Models;

namespace PopForums.Extensions
{
	public static class ViewDataDictionaries
	{
		public const string ViewDataUserKey = "PopForums.Identity.CurrentUser";

		public static void SetUserInViewData(this ViewDataDictionary viewData, User user)
		{
			viewData[ViewDataUserKey] = user;
		}
	}
}
