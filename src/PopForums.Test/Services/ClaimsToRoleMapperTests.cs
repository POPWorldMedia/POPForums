using System.Security.Claims;

namespace PopForums.Test.Services;

public class ClaimsToRoleMapperTests
{
	private IConfig _config;
	private IRoleRepository _roleRepo;
	
	private ClaimsToRoleMapper GetService()
	{
		_config = Substitute.For<IConfig>();
		_roleRepo = Substitute.For<IRoleRepository>();
		return new ClaimsToRoleMapper(_config, _roleRepo);
	}

	public class MapRoles : ClaimsToRoleMapperTests
	{
		[Fact]
		public async Task NoMappingWithNoClaims()
		{
			var service = GetService();
			_config.OAuthAdminClaimType.Returns((string)null);
			_config.OAuthAdminClaimValue.Returns((string)null);
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>();
			var savedRoles = Array.Empty<string>();
			await _roleRepo.ReplaceUserRoles(user.UserID, Arg.Do<string[]>(x => savedRoles = x));

			await service.MapRoles(user, claims);

			Assert.Empty(savedRoles);
		}
		
		[Fact]
		public async Task NoMappingWithNoMatchingClaims()
		{
			var service = GetService();
			_config.OAuthAdminClaimType.Returns("iowfhwe");
			_config.OAuthAdminClaimValue.Returns("efoijh");
			_config.OAuthModeratorClaimType.Returns("iowfhwe");
			_config.OAuthModeratorClaimValue.Returns("efoijh");
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>();
			var savedRoles = Array.Empty<string>();
			await _roleRepo.ReplaceUserRoles(user.UserID, Arg.Do<string[]>(x => savedRoles = x));

			await service.MapRoles(user, claims);

			Assert.Empty(savedRoles);
		}
		
		[Fact]
		public async Task NoMappingWithNoMatchingClaimsValues()
		{
			var service = GetService();
			_config.OAuthAdminClaimType.Returns("admin");
			_config.OAuthAdminClaimValue.Returns("yes");
			_config.OAuthModeratorClaimType.Returns("mod");
			_config.OAuthModeratorClaimValue.Returns("yes");
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>
			{
				new ("admin", "no"),
				new ("mod", "no")
			};
			var savedRoles = Array.Empty<string>();
			await _roleRepo.ReplaceUserRoles(user.UserID, Arg.Do<string[]>(x => savedRoles = x));

			await service.MapRoles(user, claims);

			Assert.Empty(savedRoles);
		}
		
		[Fact]
		public async Task AdminNameNoValueMapsAdminRole()
		{
			var service = GetService();
			_config.OAuthAdminClaimType.Returns("adminclaim");
			_config.OAuthAdminClaimValue.Returns((string)null);
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>
			{
				new ("adminclaim", string.Empty)
			};
			var savedRoles = Array.Empty<string>();
			await _roleRepo.ReplaceUserRoles(user.UserID, Arg.Do<string[]>(x => savedRoles = x));

			await service.MapRoles(user, claims);

			Assert.Contains(PermanentRoles.Admin, savedRoles);
		}
		
		[Fact]
		public async Task AdminNameWithValueMapsAdminRole()
		{
			var service = GetService();
			_config.OAuthAdminClaimType.Returns("adminclaim");
			_config.OAuthAdminClaimValue.Returns("adminvalue");
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>
			{
				new ("adminclaim", "adminvalue")
			};
			var savedRoles = Array.Empty<string>();
            await _roleRepo.ReplaceUserRoles(user.UserID, Arg.Do<string[]>(x => savedRoles = x));

			await service.MapRoles(user, claims);

			Assert.Contains(PermanentRoles.Admin, savedRoles);
		}
		
		[Fact]
		public async Task ModNameNoValueMapsModRole()
		{
			var service = GetService();
			_config.OAuthModeratorClaimType.Returns("modclaim");
			_config.OAuthModeratorClaimValue.Returns((string)null);
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>
			{
				new ("modclaim", string.Empty)
			};
			var savedRoles = Array.Empty<string>();
			await _roleRepo.ReplaceUserRoles(user.UserID, Arg.Do<string[]>(x => savedRoles = x));

			await service.MapRoles(user, claims);

			Assert.Contains(PermanentRoles.Moderator, savedRoles);
		}
		
		[Fact]
		public async Task ModNameWithValueMapsModRole()
		{
			var service = GetService();
			_config.OAuthAdminClaimType.Returns("modclaim");
			_config.OAuthAdminClaimValue.Returns("modvalue");
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>
			{
				new ("modclaim", "modvalue")
			};
			var savedRoles = Array.Empty<string>();
			await _roleRepo.ReplaceUserRoles(user.UserID, Arg.Do<string[]>(x => savedRoles = x));

			await service.MapRoles(user, claims);

			Assert.Contains(PermanentRoles.Admin, savedRoles);
		}
	}
}