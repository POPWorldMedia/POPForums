using PopIdentity;

namespace PopForums.Mvc.Areas.Forums.Models
{
	public class ExternalLoginState
	{
		public CallbackResult CallbackResult { get; set; }
		public string ReturnUrl { get; set; }
		public ProviderType ProviderType { get; set; }
	}
}