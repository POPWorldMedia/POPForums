using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using PopForums.Services;

namespace PopForums.Web
{
    public class PopForumsMiddleware
    {
	    private readonly RequestDelegate _next;

	    public PopForumsMiddleware(RequestDelegate next)
	    {
		    _next = next;
	    }

	    public async Task Invoke(HttpContext context)
	    {
		    var name = context.User.Identity.Name;
		    if (!string.IsNullOrWhiteSpace(name))
		    {
			    var userService = context.ApplicationServices.GetService<IUserService>();
			    var user = userService.GetUserByName(name);
			    if (user != null)
			    {
				    context.Items["PopForumsUser"] = user;
				    var profileService = context.ApplicationServices.GetService<IProfileService>();
				    var profile = profileService.GetProfile(user);
				    context.Items["PopForumsProfile"] = profile;
			    }
		    }
		    await _next(context);
	    }
    }
}
