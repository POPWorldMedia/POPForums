using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using PopForums.Services;

namespace PopForums.Web.Areas.Forums.Controllers
{
	[Area("Forums")]
	public class ImageController : Controller
	{
		public ImageController(IImageService imageService)
		{
			_imageService = imageService;
		}

		private readonly IImageService _imageService;

		[PopForumsAuthorizationIgnore]
		public ActionResult Avatar(int id)
		{
			return SetupImageResult(_imageService.GetAvatarImageData, _imageService.GetAvatarImageLastModification, id);
		}

		[PopForumsAuthorizationIgnore]
		public ActionResult UserImage(int id)
		{
			return SetupImageResult(_imageService.GetUserImageData, _imageService.GetUserImageLastModifcation, id);
		}

		private ActionResult SetupImageResult(Func<int, byte[]> imageDataFetch, Func<int, DateTime?> imageLastMod, int id)
		{
			var timeStamp = imageLastMod(id);
			if (!timeStamp.HasValue)
				return NotFound();
			Response.Headers["Cache-control"] = "public";
			Response.Headers["Last-modified"] = DateTime.SpecifyKind(timeStamp.Value, DateTimeKind.Utc).ToString("R");
			if (!String.IsNullOrEmpty(Request.Headers["If-Modified-Since"]))
			{
				var provider = CultureInfo.InvariantCulture;
				DateTime lastMod;
				var couldParse = DateTime.TryParseExact(Request.Headers["If-Modified-Since"], "r", provider, DateTimeStyles.None, out lastMod);
				if (couldParse && lastMod == timeStamp.Value.AddMilliseconds(-timeStamp.Value.Millisecond))
				{
					Response.StatusCode = 304;
					return Content(String.Empty);
				}
			}
			var stream = new MemoryStream(imageDataFetch(id));
			return File(stream, "image/jpeg");
		}
	}
}
