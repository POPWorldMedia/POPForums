using Microsoft.AspNetCore.Http;
using PopForums.Models;

namespace PopForums.Mvc.Areas.Forums.Models
{
    public class UserEditWithFiles : UserEdit
    {
	    public UserEditWithFiles()
	    {
		    
	    }

	    public UserEditWithFiles(User user, Profile profile) : base(user, profile)
	    {
	    }

	    public IFormFile AvatarFile { get; set; }
		public IFormFile PhotoFile { get; set; }
    }
}
