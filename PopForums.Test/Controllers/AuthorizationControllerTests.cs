using System;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Moq;
using NUnit.Framework;
using PopForums.ExternalLogin;
using PopForums.Models;
using PopForums.Controllers;
using PopForums.Services;

namespace PopForums.Test.Controllers
{
	[TestFixture]
	public class AuthorizationControllerTests
	{
		private AuthorizationController GetController()
		{
			_userService = new Mock<IUserService>();
			_owinContext = new Mock<IOwinContext>();
			_externalAuth = new Mock<IExternalAuthentication>();
			_userAssociation = new Mock<IUserAssociationManager>();
			return new AuthorizationController(_userService.Object, _owinContext.Object, _externalAuth.Object, _userAssociation.Object);
		}

		private Mock<IUserService> _userService;
		private Mock<IOwinContext> _owinContext;
		private Mock<IExternalAuthentication> _externalAuth;
		private Mock<IUserAssociationManager> _userAssociation;

		[Test]
		public void LogoutAsync()
		{
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(r => r.UserHostAddress).Returns("123");
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);

			var result = controller.LogoutAsync();

			_userService.Verify(u => u.Logout(It.IsAny<User>(), It.IsAny<string>()), Times.Once());
			var resultObject = (BasicJsonMessage)result.Data;
			Assert.IsTrue(resultObject.Result);
		}

		[Test]
		public void LoginSuccess()
		{
			const string email = "a@b.com";
			const string password = "fred";
			const bool persist = true;
			var mockContext = new Mock<HttpContextBase>();
			var mockControllerContext = new Mock<ControllerContext>();
			mockControllerContext.SetupAllProperties();
			mockControllerContext.Setup(c => c.HttpContext).Returns(mockContext.Object);
			var controller = GetController();
			controller.ControllerContext = mockControllerContext.Object;
			_userService.Setup(u => u.Login(email, password, persist, mockContext.Object)).Returns(true);

			var result = controller.Login(email, password, persist);

			_userService.Verify(u => u.Login(email, password, persist, mockContext.Object), Times.Once());
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
			var mockContext = new Mock<HttpContextBase>();
			var mockControllerContext = new Mock<ControllerContext>();
			mockControllerContext.SetupAllProperties();
			mockControllerContext.Setup(c => c.HttpContext).Returns(mockContext.Object);
			var controller = GetController();
			controller.ControllerContext = mockControllerContext.Object;
			_userService.Setup(u => u.Login(email, password, persist, mockContext.Object)).Returns(false);

			var result = controller.Login(email, password, persist);

			_userService.Verify(u => u.Login(email, password, persist, mockContext.Object), Times.Once());
			Assert.IsInstanceOf<JsonResult>(result);
			var resultObject = (BasicJsonMessage)result.Data;
			Assert.IsFalse(resultObject.Result);
			Assert.IsTrue(!String.IsNullOrEmpty(resultObject.Message));
		}

		[Test]
		public void LoginAndAssociateSuccess()
		{
			const string email = "a@b.com";
			const string password = "fred";
			var user = new User(12, DateTime.MaxValue) {Email = email};
			const bool persist = true;
			var controller = GetController();
			var contextHelper = new HttpContextHelper();
			contextHelper.MockRequest.Setup(x => x.UserHostAddress).Returns(String.Empty);
			controller.ControllerContext = new ControllerContext(contextHelper.MockContext.Object, new RouteData(), controller);
			_userService.Setup(u => u.Login(email, password, persist, contextHelper.MockContext.Object)).Returns(true);
			_userService.Setup(x => x.GetUserByEmail(email)).Returns(user);
			var authManager = new Mock<IAuthenticationManager>();
			_owinContext.Setup(x => x.Authentication).Returns(authManager.Object);
			var externalAuthResult = new ExternalAuthenticationResult();
			var authResult = Task.FromResult(externalAuthResult);
			_externalAuth.Setup(x => x.GetAuthenticationResult(authManager.Object)).Returns(authResult);

			var result = controller.LoginAndAssociate(email, password, persist).Result;

			_userAssociation.Verify(x => x.Associate(user, authResult.Result, It.IsAny<string>()));
			_userService.Verify(u => u.Login(email, password, persist, contextHelper.MockContext.Object), Times.Once());
			Assert.IsInstanceOf<JsonResult>(result);
			var resultObject = (BasicJsonMessage)result.Data;
			Assert.IsTrue(resultObject.Result);
		}

		[Test]
		public void LoginAndAssociateFail()
		{
			const string email = "a@b.com";
			const string password = "fred";
			const bool persist = true;
			var mockContext = new Mock<HttpContextBase>();
			var mockControllerContext = new Mock<ControllerContext>();
			mockControllerContext.SetupAllProperties();
			mockControllerContext.Setup(c => c.HttpContext).Returns(mockContext.Object);
			var controller = GetController();
			controller.ControllerContext = mockControllerContext.Object;
			_userService.Setup(u => u.Login(email, password, persist, mockContext.Object)).Returns(false);

			var result = controller.LoginAndAssociate(email, password, persist).Result;

			_userService.Verify(u => u.Login(email, password, persist, mockContext.Object), Times.Once());
			Assert.IsInstanceOf<JsonResult>(result);
			var resultObject = (BasicJsonMessage)result.Data;
			Assert.IsFalse(resultObject.Result);
			Assert.IsTrue(!String.IsNullOrEmpty(resultObject.Message));
		}
	}
}
