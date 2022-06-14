using PopForums.Messaging.Models;

namespace PopForums.Mvc.Areas.Forums.Controllers;

[Area("Forums")]
[ApiController]
public class ApiController : Controller
{
	private readonly INotificationAdapter _notificationAdapter;

	public ApiController(INotificationAdapter notificationAdapter)
	{
		_notificationAdapter = notificationAdapter;
	}

	[HttpPost("/Forums/Api/NotifyAward")]
	public async Task<IActionResult> NotifyAward(AwardPayload awardPayload)
	{
		if (awardPayload == null)
			return BadRequest();
		await _notificationAdapter.Award(awardPayload.Title, awardPayload.UserID, awardPayload.TenantID);
		return Ok();
	}
}