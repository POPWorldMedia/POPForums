using System.Web.Mvc;
using PopForums.Models;

namespace PopForums.Extensions
{
	public static class WebViewPages
	{
		public static User GetUserFromViewData(this WebViewPage viewPage)
		{
			return (User)viewPage.ViewData[ViewDataDictionaries.ViewDataUserKey];
		}

		public static bool IsUserInRole(this WebViewPage viewPage, string role)
		{
			var user = viewPage.GetUserFromViewData();
			if (user == null)
				return false;
			return user.IsInRole(role);
		}
	}
}
