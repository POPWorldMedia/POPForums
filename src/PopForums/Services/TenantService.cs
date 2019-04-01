using System;

namespace PopForums.Services
{
	public interface ITenantService
	{
		void SetTenant(string tenantID);
		string GetTenant();
	}

	public class TenantService : ITenantService
	{
		public void SetTenant(string tenantID)
		{
			throw new NotImplementedException();
		}

		public string GetTenant()
		{
			return string.Empty;
		}
	}
}