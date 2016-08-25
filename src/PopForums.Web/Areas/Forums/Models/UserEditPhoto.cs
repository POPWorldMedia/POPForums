using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PopForums.Models;

namespace PopForums.Web.Areas.Forums.Models
{
    public class UserEditPhoto
	{
		public UserEditPhoto() { }

		public UserEditPhoto(Profile profile)
		{
			AvatarID = profile.AvatarID;
			ImageID = profile.ImageID;
		}

		public int? AvatarID { get; set; }
		public int? ImageID { get; set; }
		public bool? IsImageApproved { get; set; }
		public bool DeleteAvatar { get; set; }
		public bool DeleteImage { get; set; }
		public IFormFile AvatarFile { get; set; }
		public IFormFile PhotoFile { get; set; }
	}
}
