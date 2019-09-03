using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopForums.Mvc.Areas.Forums.Authorization;
using PopForums.Services;

namespace PopForums.Mvc.Areas.Forums.Controllers
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
		public async Task<ActionResult> Avatar(int id)
		{
			return await SetupImageResult(_imageService.GetAvatarImageData, _imageService.GetAvatarImageLastModification, id);
		}

		[PopForumsAuthorizationIgnore]
		public async Task<ActionResult> UserImage(int id)
		{
			return await SetupImageResult(_imageService.GetUserImageData, _imageService.GetUserImageLastModifcation, id);
		}

		private async Task<ActionResult> SetupImageResult(Func<int, Task<byte[]>> imageDataFetch, Func<int, Task<DateTime?>> imageLastMod, int id)
		{
			var timeStamp = await imageLastMod(id);
			if (!timeStamp.HasValue)
				return NotFound();
			Response.Headers["Cache-control"] = "public";
			Response.Headers["Last-modified"] = DateTime.SpecifyKind(timeStamp.Value, DateTimeKind.Utc).ToString("R");
			if (!string.IsNullOrEmpty(Request.Headers["If-Modified-Since"]))
			{
				var provider = CultureInfo.InvariantCulture;
				var couldParse = DateTime.TryParseExact(Request.Headers["If-Modified-Since"], "r", provider, DateTimeStyles.None, out var lastMod);
				if (couldParse && lastMod == timeStamp.Value.AddMilliseconds(-timeStamp.Value.Millisecond))
				{
					Response.StatusCode = 304;
					return Content(string.Empty);
				}
			}
			var stream = new MemoryStream(await imageDataFetch(id));
			return File(stream, "image/jpeg");
		}
	}
}
