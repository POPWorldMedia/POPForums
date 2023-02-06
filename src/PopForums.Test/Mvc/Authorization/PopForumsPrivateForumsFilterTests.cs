using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using PopForums.Mvc.Areas.Forums.Authorization;

namespace PopForums.Test.Mvc.Authorization;

public class PopForumsPrivateForumsFilterTests
{
	private PopForumsPrivateForumsFilter GetFilter()
	{
		_userRetrievalShim = new Mock<IUserRetrievalShim>();
		_settingsManager = new Mock<ISettingsManager>();
		return new PopForumsPrivateForumsFilter(_userRetrievalShim.Object, _settingsManager.Object);
	}

	private ActionExecutingContext GetContext()
	{
		return new ActionExecutingContext(new ActionContext{HttpContext = new DefaultHttpContext(), RouteData = new RouteData(), ActionDescriptor = new ActionDescriptor()}, new List<IFilterMetadata>(), new Dictionary<string, object>(), null);
	}

	private Mock<IUserRetrievalShim> _userRetrievalShim;
	private Mock<ISettingsManager> _settingsManager;

	public class OnActionExecuting : PopForumsPrivateForumsFilterTests
	{
		[Fact]
		public void DoesNothingIfSettingIsFalse()
		{
			var filter = GetFilter();
			_settingsManager.Setup(x => x.Current.IsPrivateForumInstance).Returns(false);
			var context = GetContext();
			
			filter.OnActionExecuting(context);
			
			_userRetrievalShim.Verify(x => x.GetUser(), Times.Never);
			Assert.Null(context.Result);
		}

		[Fact]
		public void DoesNothingIfSettingIsTrueAndUserPresent()
		{
			var filter = GetFilter();
			_settingsManager.Setup(x => x.Current.IsPrivateForumInstance).Returns(true);
			var context = GetContext();
			var user = new User();
			_userRetrievalShim.Setup(x => x.GetUser()).Returns(user);
			
			filter.OnActionExecuting(context);
			
			_userRetrievalShim.Verify(x => x.GetUser(), Times.Once);
			Assert.Null(context.Result);
		}

		[Fact]
		public void RedirectIfSettingIsTrueAndNoUserPresent()
		{
			var filter = GetFilter();
			_settingsManager.Setup(x => x.Current.IsPrivateForumInstance).Returns(true);
			var context = GetContext();
			
			filter.OnActionExecuting(context);
			
			_userRetrievalShim.Verify(x => x.GetUser(), Times.Once);
			Assert.IsType<RedirectToActionResult>(context.Result);
		}
	}
}