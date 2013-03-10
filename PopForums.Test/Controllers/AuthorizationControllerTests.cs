using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using PopForums.Models;
using PopForums.Controllers;
using PopForums.Services;

namespace PopForums.Test.Controllers
{
	[TestFixture]
	public class AuthorizationControllerTests
	{
		[Test]
		public void LogoutAsync()
		{
			var userManagerMock = new Mock<IUserService>();
			var controller = new AuthorizationController(userManagerMock.Object);
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns("123");
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			var result = controller.LogoutAsync();
			userManagerMock.Verify(u => u.Logout(It.IsAny<User>(), It.IsAny<string>()), Times.Once());
			var resultObject = (BasicJsonMessage)result.Data;
			Assert.IsTrue(resultObject.Result);
		}

		[Test]
		public void LoginSuccess()
		{
			const string email = "a@b.com";
			const string password = "fred";
			const bool persist = true;
			var userManagerMock = new Mock<IUserService>();
			var mockContext = new Mock<HttpContextBase>();
			var mockControllerContext = new Mock<ControllerContext>();
			mockControllerContext.SetupAllProperties();
			mockControllerContext.Setup(c => c.HttpContext).Returns(mockContext.Object);
			userManagerMock.Setup(u => u.Login(email, password, persist, mockContext.Object)).Returns(true);
			var controller = new AuthorizationController(userManagerMock.Object) {ControllerContext = mockControllerContext.Object};
			var result = controller.Login(email, password, persist);
			userManagerMock.Verify(u => u.Login(email, password, persist, mockContext.Object), Times.Once());
			Assert.IsInstanceOf<JsonResult>(result);
			var resultObject = (BasicJsonMessage)result.Data;
			Assert.IsTrue(resultObject.Result);
		}

		[Test]
		public void LoginFail()
		{
			const string email = "a@b.com";
			const string password = "fred";
			const bool persist = true;
			var userManagerMock = new Mock<IUserService>();
			var mockContext = new Mock<HttpContextBase>();
			var mockControllerContext = new Mock<ControllerContext>();
			mockControllerContext.SetupAllProperties();
			mockControllerContext.Setup(c => c.HttpContext).Returns(mockContext.Object);
			userManagerMock.Setup(u => u.Login(email, password, persist, mockContext.Object)).Returns(false);
			var controller = new AuthorizationController(userManagerMock.Object) {ControllerContext = mockControllerContext.Object};
			var result = controller.Login(email, password, persist);
			userManagerMock.Verify(u => u.Login(email, password, persist, mockContext.Object), Times.Once());
			Assert.IsInstanceOf<JsonResult>(result);
			var resultObject = (BasicJsonMessage)result.Data;
			Assert.IsFalse(resultObject.Result);
			Assert.IsTrue(!String.IsNullOrEmpty(resultObject.Message));
		}
	}
}
