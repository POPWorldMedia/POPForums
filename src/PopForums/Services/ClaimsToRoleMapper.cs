using System.Security.Claims;

namespace PopForums.Services;

public interface IClaimsToRoleMapper
{
	Task MapRoles(User user, IEnumerable<Claim> claims);
}

public class ClaimsToRoleMapper : IClaimsToRoleMapper
{
	private readonly IConfig _config;
	private readonly IRoleRepository _roleRepository;

	public ClaimsToRoleMapper(IConfig config, IRoleRepository roleRepository)
	{
		_config = config;
		_roleRepository = roleRepository;
	}

	public async Task MapRoles(User user, IEnumerable<Claim> claims)
	{
		bool isAdmin = false, isModerator = false;
		var claimsList = claims.ToList();
		var adminClaims = claimsList.Where(x => x.Type == _config.OAuthAdminClaimName).ToList();
		if ((string.IsNullOrEmpty(_config.OAuthAdminClaimValue) && adminClaims.Any()) 
		    || adminClaims.Any(x => x.Value == _config.OAuthAdminClaimValue))
			isAdmin = true;
		var modClaims = claimsList.Where(x => x.Type == _config.OAuthModeratorClaimName).ToList();
		if ((string.IsNullOrEmpty(_config.OAuthModeratorClaimValue) && modClaims.Any()) 
		    || modClaims.Any(x => x.Value == _config.OAuthModeratorClaimValue))
			isModerator = true;

		var newRoles = new List<string>();
		if (isAdmin)
			newRoles.Add(PermanentRoles.Admin);
		if (isModerator)
			newRoles.Add(PermanentRoles.Moderator);
		await _roleRepository.ReplaceUserRoles(user.UserID, newRoles.ToArray());
	}
}