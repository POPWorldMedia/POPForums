using Microsoft.AspNet.Http;
using PopForums.Models;

namespace PopForums.Web.Areas.Forums.Models
{
    public class UserEditWithFiles : UserEdit
    {
	    public UserEditWithFiles(User user, Profile profile) : base(user, profile)
	    {
	    }

	    public IFormFile AvatarFile { get; set; }
		public IFormFile PhotoFile { get; set; }
    }
}
