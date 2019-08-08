using PopIdentity;

namespace PopForums.Mvc.Areas.Forums.Models
{
	public class ExternalLoginState
	{
		public ResultData ResultData { get; set; }
		public string ReturnUrl { get; set; }
		public ProviderType ProviderType { get; set; }
	}
}