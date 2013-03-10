using System.Web;
using PopForums.Models;

namespace PopForums.Services
{
	public interface IUserSessionService
	{
		void ProcessUserRequest(User user, HttpContextBase context);
		void CleanUpExpiredSessions();
		int GetTotalSessionCount();
	}
}