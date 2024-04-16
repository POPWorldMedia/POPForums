using PopForums.Services.Interfaces;

namespace PopForums.Services;

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