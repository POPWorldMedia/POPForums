﻿using System.Net.Mime;
using PopForums.Mvc.Areas.Forums.Authentication;

namespace PopForums.Mvc.Areas.Forums.Controllers;

[Area("Forums")]
[TypeFilter(typeof(PopForumsPrivateForumsFilter))]
public class ImageController : Controller
{
	public ImageController(IImageService imageService, IUserRetrievalShim userRetrievalShim, IPostImageService postImageService, ISettingsManager settingsManager)
	{
		_imageService = imageService;
		_userRetrievalShim = userRetrievalShim;
		_postImageService = postImageService;
		_settingsManager = settingsManager;
	}

	private readonly IImageService _imageService;
	private readonly IUserRetrievalShim _userRetrievalShim;
	private readonly IPostImageService _postImageService;
	private readonly ISettingsManager _settingsManager;

	[PopForumsAuthenticationIgnore]
	public async Task<ActionResult> Avatar(int id)
	{
		return await SetupImageResult(_imageService.GetAvatarImageStream, _imageService.GetAvatarImageLastModification, id);
	}

	[PopForumsAuthenticationIgnore]
	public async Task<ActionResult> UserImage(int id)
	{
		return await SetupImageResult(_imageService.GetUserImageStream, _imageService.GetUserImageLastModifcation, id);
	}

	private async Task<ActionResult> SetupImageResult(Func<int, Task<IStreamResponse>> imageStreamFetch, Func<int, Task<DateTime?>> imageLastMod, int id)
	{
		var timeStamp = await imageLastMod(id);
		if (!timeStamp.HasValue)
			return NotFound();
		Response.Headers["Cache-control"] = "private";
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
		var streamResponse = await imageStreamFetch(id);
		if (streamResponse == null)
			return NotFound();
		Response.RegisterForDispose(streamResponse);
		return File(streamResponse.Stream, "image/jpeg");
	}

	public async Task<ActionResult> PostImage(string id)
	{
		var postImageSansData = await _postImageService.GetWithoutData(id);
		if (postImageSansData == null)
			return NotFound();
		Response.Headers["Cache-control"] = "private";
		Response.Headers["Last-modified"] = DateTime.SpecifyKind(postImageSansData.TimeStamp, DateTimeKind.Utc).ToString("R");
		if (!string.IsNullOrEmpty(Request.Headers["If-Modified-Since"]))
		{
			var provider = CultureInfo.InvariantCulture;
			var couldParse = DateTime.TryParseExact(Request.Headers["If-Modified-Since"], "r", provider, DateTimeStyles.None, out var lastMod);
			if (couldParse && lastMod == postImageSansData.TimeStamp.AddMilliseconds(-postImageSansData.TimeStamp.Millisecond))
			{
				Response.StatusCode = 304;
				return Content(string.Empty);
			}
		}
		
		var streamResponse = await _postImageService.GetImageStream(id);
		if (streamResponse == null)
			return NotFound();
		Response.RegisterForDispose(streamResponse);
		return File(streamResponse.Stream, postImageSansData.ContentType);
	}

	[HttpPost]
	public async Task<ActionResult> UploadPostImage()
	{
		var user = _userRetrievalShim.GetUser();
		if (user == null)
			return Unauthorized();
		if (!_settingsManager.Current.AllowImages)
			return BadRequest();
		var file = Request.Form.Files[0];
		if (file.ContentType != MediaTypeNames.Image.Jpeg 
		    && file.ContentType != MediaTypeNames.Image.Gif
		    && file.ContentType != "image/png")
			return BadRequest();
		var stream = file.OpenReadStream();
		var bytes = stream.ToBytes();
		var isOk = _postImageService.ProcessImageIsOk(bytes, file.ContentType);
		if (!isOk)
			return BadRequest();
		var url = await _postImageService.PersistAndGetPayload();
		return Ok(url);
	}
}