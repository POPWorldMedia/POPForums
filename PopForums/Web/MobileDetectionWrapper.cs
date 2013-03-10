using System.Web;

namespace PopForums.Web
{
	public class MobileDetectionWrapper : IMobileDetectionWrapper
	{
		public bool IsMobileDevice(HttpContextBase context)
		{
			return context.Request.Browser.IsMobileDevice;
		}
	}
}
