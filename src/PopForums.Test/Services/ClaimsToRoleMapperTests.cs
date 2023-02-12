using System.Security.Claims;

namespace PopForums.Test.Services;

public class ClaimsToRoleMapperTests
{
	private Mock<IConfig> _config;
	private Mock<IRoleRepository> _roleRepo;
	
	private ClaimsToRoleMapper GetService()
	{
		_config = new Mock<IConfig>();
		_roleRepo = new Mock<IRoleRepository>();
		return new ClaimsToRoleMapper(_config.Object, _roleRepo.Object);
	}

	public class MapRoles : ClaimsToRoleMapperTests
	{
		[Fact]
		public async void NoMappingWithNoClaims()
		{
			var service = GetService();
			_config.Setup(x => x.OAuthAdminClaimType).Returns((string)null);
			_config.Setup(x => x.OAuthAdminClaimValue).Returns((string)null);
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>();
			string[] savedRoles = Array.Empty<string>();
			_roleRepo.Setup(x => x.ReplaceUserRoles(user.UserID, It.IsAny<string[]>()))
				.Callback<int, string[]>((u, r) => savedRoles = r);

			await service.MapRoles(user, claims);

			Assert.Empty(savedRoles);
		}
		
		[Fact]
		public async void NoMappingWithNoMatchingClaims()
		{
			var service = GetService();
			_config.Setup(x => x.OAuthAdminClaimType).Returns("iowfhwe");
			_config.Setup(x => x.OAuthAdminClaimValue).Returns("efoijh");
			_config.Setup(x => x.OAuthModeratorClaimType).Returns("iowfhwe");
			_config.Setup(x => x.OAuthModeratorClaimValue).Returns("efoijh");
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>();
			string[] savedRoles = Array.Empty<string>();
			_roleRepo.Setup(x => x.ReplaceUserRoles(user.UserID, It.IsAny<string[]>()))
				.Callback<int, string[]>((u, r) => savedRoles = r);

			await service.MapRoles(user, claims);

			Assert.Empty(savedRoles);
		}
		
		[Fact]
		public async void NoMappingWithNoMatchingClaimsValues()
		{
			var service = GetService();
			_config.Setup(x => x.OAuthAdminClaimType).Returns("admin");
			_config.Setup(x => x.OAuthAdminClaimValue).Returns("yes");
			_config.Setup(x => x.OAuthModeratorClaimType).Returns("mod");
			_config.Setup(x => x.OAuthModeratorClaimValue).Returns("yes");
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>
			{
				new ("admin", "no"),
				new ("mod", "no")
			};
			string[] savedRoles = Array.Empty<string>();
			_roleRepo.Setup(x => x.ReplaceUserRoles(user.UserID, It.IsAny<string[]>()))
				.Callback<int, string[]>((u, r) => savedRoles = r);

			await service.MapRoles(user, claims);

			Assert.Empty(savedRoles);
		}
		
		[Fact]
		public async void AdminNameNoValueMapsAdminRole()
		{
			var service = GetService();
			_config.Setup(x => x.OAuthAdminClaimType).Returns("adminclaim");
			_config.Setup(x => x.OAuthAdminClaimValue).Returns((string)null);
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>
			{
				new ("adminclaim", string.Empty)
			};
			string[] savedRoles = Array.Empty<string>();
			_roleRepo.Setup(x => x.ReplaceUserRoles(user.UserID, It.IsAny<string[]>()))
				.Callback<int, string[]>((u, r) => savedRoles = r);

			await service.MapRoles(user, claims);

			Assert.Contains(PermanentRoles.Admin, savedRoles);
		}
		
		[Fact]
		public async void AdminNameWithValueMapsAdminRole()
		{
			var service = GetService();
			_config.Setup(x => x.OAuthAdminClaimType).Returns("adminclaim");
			_config.Setup(x => x.OAuthAdminClaimValue).Returns("adminvalue");
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>
			{
				new ("adminclaim", "adminvalue")
			};
			string[] savedRoles = Array.Empty<string>();
			_roleRepo.Setup(x => x.ReplaceUserRoles(user.UserID, It.IsAny<string[]>()))
				.Callback<int, string[]>((u, r) => savedRoles = r);

			await service.MapRoles(user, claims);

			Assert.Contains(PermanentRoles.Admin, savedRoles);
		}
		
		[Fact]
		public async void ModNameNoValueMapsModRole()
		{
			var service = GetService();
			_config.Setup(x => x.OAuthModeratorClaimType).Returns("modclaim");
			_config.Setup(x => x.OAuthModeratorClaimValue).Returns((string)null);
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>
			{
				new ("modclaim", string.Empty)
			};
			string[] savedRoles = Array.Empty<string>();
			_roleRepo.Setup(x => x.ReplaceUserRoles(user.UserID, It.IsAny<string[]>()))
				.Callback<int, string[]>((u, r) => savedRoles = r);

			await service.MapRoles(user, claims);

			Assert.Contains(PermanentRoles.Moderator, savedRoles);
		}
		
		[Fact]
		public async void ModNameWithValueMapsModRole()
		{
			var service = GetService();
			_config.Setup(x => x.OAuthAdminClaimType).Returns("modclaim");
			_config.Setup(x => x.OAuthAdminClaimValue).Returns("modvalue");
			var user = new User { Roles = new List<string>(), UserID = 123 };
			var claims = new List<Claim>
			{
				new ("modclaim", "modvalue")
			};
			string[] savedRoles = Array.Empty<string>();
			_roleRepo.Setup(x => x.ReplaceUserRoles(user.UserID, It.IsAny<string[]>()))
				.Callback<int, string[]>((u, r) => savedRoles = r);

			await service.MapRoles(user, claims);

			Assert.Contains(PermanentRoles.Admin, savedRoles);
		}
	}
}