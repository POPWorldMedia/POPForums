using System;
using Microsoft.AspNetCore.Mvc;
using PopForums.Mvc.Areas.Forums.Models;
using PopForums.Mvc.Areas.Forums.Services;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.ViewComponents
{
    public class UserNavigationViewComponent : ViewComponent
    {
	    private readonly IUserRetrievalShim _userRetrievalShim;
	    private readonly IPrivateMessageService _privateMessageService;

	    public UserNavigationViewComponent(IUserRetrievalShim userRetrievalShim, IPrivateMessageService privateMessageService)
	    {
		    _userRetrievalShim = userRetrievalShim;
		    _privateMessageService = privateMessageService;
	    }

	    public IViewComponentResult Invoke()
	    {
		    var container = new UserNavigationContainer();
            container.User = _userRetrievalShim.GetUser(HttpContext);
		    if (container.User != null)
		    {
			    var count = _privateMessageService.GetUnreadCount(container.User);
			    if (count > 0)
				    container.PMCount = String.Format("<span class=\"badge\">{0}</span>", count);
		    }
		    return View(container);
	    }
    }
}
