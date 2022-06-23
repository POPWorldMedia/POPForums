using PopForums.Messaging.Models;

namespace PopForums.Mvc.Areas.Forums.Controllers;

[Area("Forums")]
[ApiController]
public class ApiController : Controller
{
	private readonly INotificationAdapter _notificationAdapter;
	private readonly IConfig _config;

	public ApiController(INotificationAdapter notificationAdapter, IConfig config)
	{
		_notificationAdapter = notificationAdapter;
		_config = config;
	}
	
	[HttpPost("/Forums/Api/NotifyAward")]
	public async Task<IActionResult> NotifyAward(AwardPayload awardPayload)
	{
		var hash = _config.QueueConnectionString.GetSHA256Hash();
		var result = HttpContext.Request.Headers.TryGetValue(NotificationTunnel.HeaderName, out var headerValue);
		if (headerValue != hash)
			return Unauthorized();
		if (awardPayload == null)
			return BadRequest();
		await _notificationAdapter.Award(awardPayload.Title, awardPayload.UserID, awardPayload.TenantID);
		return Ok();
	}
}