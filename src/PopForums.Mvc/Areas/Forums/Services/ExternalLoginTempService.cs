using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PopIdentity;

namespace PopForums.Mvc.Areas.Forums.Services
{
	public interface IExternalLoginTempService
	{
		void Persist(CallbackResult callbackResult);
		CallbackResult Read();
		void Remove();
	}

	public class ExternalLoginTempService : IExternalLoginTempService
	{
		private readonly IDataProtectionProvider _dataProtectionProvider;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private const string CookieKey = "pf_authtemp";

		public ExternalLoginTempService(IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor httpContextAccessor)
		{
			_dataProtectionProvider = dataProtectionProvider;
			_httpContextAccessor = httpContextAccessor;
		}

		public void Persist(CallbackResult callbackResult)
		{
			var protector = _dataProtectionProvider.CreateProtector(nameof(ExternalLoginTempService));
			var serializedResult = JsonConvert.SerializeObject(callbackResult);
			var encryptedResult = protector.Protect(serializedResult);
			_httpContextAccessor.HttpContext.Response.Cookies.Append(CookieKey, encryptedResult);
		}

		public CallbackResult Read()
		{
			var protector = _dataProtectionProvider.CreateProtector(nameof(ExternalLoginTempService));
			var encryptedTempAuth = _httpContextAccessor.HttpContext.Request.Cookies[CookieKey];
			var decryptedSerialized = protector.Unprotect(encryptedTempAuth);
			var result = JsonConvert.DeserializeObject<CallbackResult>(decryptedSerialized);
			return result;
		}

		public void Remove()
		{
			_httpContextAccessor.HttpContext.Response.Cookies.Delete(CookieKey);
		}
	}
}