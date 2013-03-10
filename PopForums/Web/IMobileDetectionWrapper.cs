using System.Web;

namespace PopForums.Web
{
	public interface IMobileDetectionWrapper
	{
		bool IsMobileDevice(HttpContextBase context);
	}
}