using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PopForums.Mvc.Areas.Forums.Models;

namespace PopForums.Mvc.Areas.Forums.Services
{
	public interface IExternalLoginTempService
	{
		void Persist(ExternalLoginState externalLoginState);
		ExternalLoginState Read();
		void Remove();
	}

	public class ExternalLoginTempService : IExternalLoginTempService
	{
		private readonly IDataProtectionProvider _dataProtectionProvider;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private const string CookieKey = "pf_temploginstate";

		public ExternalLoginTempService(IDataProtectionProvider dataProtectionProvider, IHttpContextAccessor httpContextAccessor)
		{
			_dataProtectionProvider = dataProtectionProvider;
			_httpContextAccessor = httpContextAccessor;
		}

		public void Persist(ExternalLoginState externalLoginState)
		{
			var protector = _dataProtectionProvider.CreateProtector(nameof(ExternalLoginTempService));
			var serializedResult = JsonConvert.SerializeObject(externalLoginState);
			var encryptedResult = protector.Protect(serializedResult);
			_httpContextAccessor.HttpContext.Response.Cookies.Append(CookieKey, encryptedResult);
		}

		public ExternalLoginState Read()
		{
			var protector = _dataProtectionProvider.CreateProtector(nameof(ExternalLoginTempService));
			var encryptedTempAuth = _httpContextAccessor.HttpContext.Request.Cookies[CookieKey];
			if (string.IsNullOrEmpty(encryptedTempAuth))
			{
				return null;
			}
			var decryptedSerialized = protector.Unprotect(encryptedTempAuth);
			var result = JsonConvert.DeserializeObject<ExternalLoginState>(decryptedSerialized);
			return result;
		}

		public void Remove()
		{
			_httpContextAccessor.HttpContext.Response.Cookies.Delete(CookieKey);
		}
	}
}