namespace PopForums.Services.Interfaces;

public interface ITenantService
{
    void SetTenant(string tenantID);

    string GetTenant();
}
