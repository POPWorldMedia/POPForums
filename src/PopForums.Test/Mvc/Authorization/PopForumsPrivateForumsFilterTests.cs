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
		_userRetrievalShim = Substitute.For<IUserRetrievalShim>();
		_settingsManager = Substitute.For<ISettingsManager>();
		_config = Substitute.For<IConfig>();
		return new PopForumsPrivateForumsFilter(_userRetrievalShim, _settingsManager, _config);
	}

	private ActionExecutingContext GetContext()
	{
		return new ActionExecutingContext(new ActionContext{HttpContext = new DefaultHttpContext(), RouteData = new RouteData(), ActionDescriptor = new ActionDescriptor()}, new List<IFilterMetadata>(), new Dictionary<string, object>(), null);
	}

	private IUserRetrievalShim _userRetrievalShim;
	private ISettingsManager _settingsManager;
	private IConfig _config;

	public class OnActionExecuting : PopForumsPrivateForumsFilterTests
	{
		[Fact]
		public void DoesNothingIfSettingIsFalseAndOAuthOnlyIsFalse()
		{
			var filter = GetFilter();
			_config.IsOAuthOnly.Returns(false);
			_settingsManager.Current.IsPrivateForumInstance.Returns(false);
			var context = GetContext();
			
			filter.OnActionExecuting(context);
			
			_userRetrievalShim.DidNotReceive().GetUser();
			Assert.Null(context.Result);
		}

		[Fact]
		public void DoesNothingIfSettingIsTrueAndUserPresent()
		{
			var filter = GetFilter();
			_config.IsOAuthOnly.Returns(false);
			_settingsManager.Current.IsPrivateForumInstance.Returns(true);
			var context = GetContext();
			var user = new User();
			_userRetrievalShim.GetUser().Returns(user);
			
			filter.OnActionExecuting(context);
			
			_userRetrievalShim.Received().GetUser();
			Assert.Null(context.Result);
		}

		[Fact]
		public void DoesNothingIfOAuthOnlyIsTrueAndUserPresent()
		{
			var filter = GetFilter();
			_config.IsOAuthOnly.Returns(true);
			_settingsManager.Current.IsPrivateForumInstance.Returns(false);
			var context = GetContext();
			var user = new User();
			_userRetrievalShim.GetUser().Returns(user);
			
			filter.OnActionExecuting(context);
			
			_userRetrievalShim.Received().GetUser();
			Assert.Null(context.Result);
		}

		[Fact]
		public void RedirectIfSettingIsTrueAndNoUserPresent()
		{
			var filter = GetFilter();
			_config.IsOAuthOnly.Returns(false);
			_settingsManager.Current.IsPrivateForumInstance.Returns(true);
			var context = GetContext();
			
			filter.OnActionExecuting(context);
			
			_userRetrievalShim.Received().GetUser();
			Assert.IsType<RedirectToActionResult>(context.Result);
		}

		[Fact]
		public void RedirectIfOAuthOnlyIsTrueAndNoUserPresent()
		{
			var filter = GetFilter();
			_config.IsOAuthOnly.Returns(true);
			_settingsManager.Current.IsPrivateForumInstance.Returns(false);
			var context = GetContext();
			
			filter.OnActionExecuting(context);
			
			_userRetrievalShim.Received().GetUser();
			Assert.IsType<RedirectToActionResult>(context.Result);
		}
	}
}