using System.Web;
using System.Web.WebPages;

namespace PopForums.Web
{
	public class MobileDetectionWrapper : IMobileDetectionWrapper
	{
		public bool IsMobileDevice(HttpContextBase context)
		{
			return context.Request.Browser.IsMobileDevice || context.GetOverriddenBrowser().IsMobileDevice;
		}
	}
}
