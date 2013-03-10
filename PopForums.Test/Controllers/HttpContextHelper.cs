using System.Web;
using System.Web.Routing;
using Moq;

namespace PopForums.Test.Controllers
{
	public class HttpContextHelper
	{
		public HttpContextHelper()
		{
			MockContext = new Mock<HttpContextBase>();
			MockRequest = new Mock<HttpRequestBase>();
			MockResponse = new Mock<HttpResponseBase>();
			MockRequestContext = new Mock<RequestContext>();

			MockContext.SetupAllProperties();
			MockContext.Setup(c => c.Request).Returns(MockRequest.Object);
			MockContext.Setup(c => c.Response).Returns(MockResponse.Object);
			MockRequestContext.SetupAllProperties();
			MockRequestContext.Setup(x => x.HttpContext).Returns(MockContext.Object);
			var routeData = new Mock<RouteData>();
			routeData.SetupAllProperties();
			MockRequestContext.Setup(x => x.RouteData).Returns(routeData.Object);
			MockRequest.SetupAllProperties();
			MockRequest.Setup(c => c.RequestContext).Returns(MockRequestContext.Object);
		}

		public Mock<HttpContextBase> MockContext { get; set; }
		public Mock<HttpRequestBase> MockRequest { get; set; }
		public Mock<HttpResponseBase> MockResponse { get; set; }
		public Mock<RequestContext> MockRequestContext { get; set; }
	}
}