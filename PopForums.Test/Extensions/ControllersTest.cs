using System;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using PopForums.Extensions;
using PopForums.Test.Controllers;

namespace PopForums.Test.Extensions
{
	[TestFixture]
	public class ControllersTest
	{
		[Test]
		public void ForbiddenActionResult()
		{
			var controller = new TestController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var guid = Guid.NewGuid();
			var result = controller.Forbidden("blah", guid);
			Assert.IsInstanceOf<ViewResult>(result);
			Assert.AreEqual("blah", result.ViewName);
			Assert.AreEqual(guid, result.ViewData.Model);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int) HttpStatusCode.Forbidden);
		}

		[Test]
		public void NotFoundActionResult()
		{
			var controller = new TestController();
			var contextHelper = new HttpContextHelper();
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var guid = Guid.NewGuid();
			var result = controller.NotFound("blah", guid);
			Assert.IsInstanceOf<ViewResult>(result);
			Assert.AreEqual("blah", result.ViewName);
			Assert.AreEqual(guid, result.ViewData.Model);
			contextHelper.MockResponse.VerifySet(r => r.StatusCode = (int)HttpStatusCode.NotFound);
		}

		[Test]
		public void FullUrlHelper()
		{
		}
	}

	public class TestController : Controller
	{
		
	}
}