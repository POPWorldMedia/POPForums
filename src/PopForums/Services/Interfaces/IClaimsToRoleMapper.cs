namespace PopForums.Services.Interfaces;

public interface IClaimsToRoleMapper
{
    Task MapRoles(User user, IEnumerable<Claim> claims);
}
