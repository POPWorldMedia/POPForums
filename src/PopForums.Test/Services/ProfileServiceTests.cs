using System;
using Moq;
using Xunit;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PopForums.Test.Services
{
	public class ProfileServiceTests
	{
		private Mock<IProfileRepository> _profileRepo;
		private Mock<ITextParsingService> _textParsingService;
		private Mock<IPointLedgerRepository> _pointLedger;

		private ProfileService GetService()
		{
			_profileRepo = new Mock<IProfileRepository>();
			_textParsingService = new Mock<ITextParsingService>();
			_pointLedger = new Mock<IPointLedgerRepository>();
			return new ProfileService(_profileRepo.Object, _textParsingService.Object, _pointLedger.Object);
		}

		[Fact]
		public async Task GetProfile()
		{
			var service = GetService();
			var profile = new Profile { UserID = 123, Location = "Cleveland" };
			var user = UserServiceTests.GetDummyUser("Jeff", "a@b.com");
			_profileRepo.Setup(p => p.GetProfile(user.UserID)).ReturnsAsync(profile);
			var result = await service.GetProfile(user);
			Assert.Equal(profile, result);
			_profileRepo.Verify(p => p.GetProfile(user.UserID), Times.Once());
		}

		[Fact]
		public async Task GetProfileReturnsNullForNullUser()
		{
			var service = GetService();
			var result = await service.GetProfile(null);
			Assert.Null(result);
		}

		[Fact]
		public async Task GetProfileForEditParsesSig()
		{
			var service = GetService();
			var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = "blah" };
			var user = UserServiceTests.GetDummyUser("Jeff", "a@b.com");
			_profileRepo.Setup(p => p.GetProfile(user.UserID)).ReturnsAsync(profile);
			_textParsingService.Setup(t => t.ClientHtmlToForumCode("blah")).Returns("parsed");
			var result = await service.GetProfileForEdit(user);
			Assert.Equal("parsed", result.Signature);
			_profileRepo.Verify(p => p.GetProfile(user.UserID), Times.Once());
		}

		[Fact]
		public async Task GetProfileForEditParsesSigGuardForNull()
		{
			var service = GetService();
			var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = null };
			var user = UserServiceTests.GetDummyUser("Jeff", "a@b.com");
			_profileRepo.Setup(p => p.GetProfile(user.UserID)).ReturnsAsync(profile);

			var result = await service.GetProfileForEdit(user);

			_textParsingService.Verify(x => x.ClientHtmlToForumCode(It.IsAny<string>()), Times.Never);
			Assert.Equal(string.Empty, result.Signature);
			_profileRepo.Verify(p => p.GetProfile(user.UserID), Times.Once());
		}

		[Fact]
		public async Task CreateFromProfileObject()
		{
			var service = GetService();
			var profile = new Profile { UserID = 123, Location = "Cleveland" };
			_profileRepo.Setup(p => p.Create(profile));
			await service.Create(profile);
			_profileRepo.Verify(p => p.Create(profile), Times.Once());
		}

		[Fact]
		public async Task CreateFromProfileThrowsWithoutUserID()
		{
			var service = GetService();
			var profile = new Profile();
			await Assert.ThrowsAsync<Exception>(() => service.Create(profile));
			_profileRepo.Verify(p => p.Create(profile), Times.Never());
		}

		[Fact]
		public async Task CreateFromUserAndSignupData()
		{
			var service = GetService();
			var user = UserServiceTests.GetDummyUser("Jeff", "a@b.com");
			var signupData = new SignupData {TimeZone = -5, IsSubscribed = true, IsTos = true, IsDaylightSaving = true};
			_profileRepo.Setup(r => r.Create(It.Is<Profile>(p =>
			                                                   p.UserID == user.UserID &&
			                                                   p.TimeZone == signupData.TimeZone &&
			                                                   p.IsSubscribed == signupData.IsSubscribed &&
			                                                   p.IsTos == signupData.IsTos &&
			                                                   p.IsDaylightSaving == signupData.IsDaylightSaving))).Verifiable();
			var result = await service.Create(user, signupData);
			Assert.Equal(user.UserID, result.UserID);
			Assert.Equal(signupData.IsDaylightSaving, result.IsDaylightSaving);
			Assert.Equal(signupData.IsSubscribed, result.IsSubscribed);
			Assert.Equal(signupData.IsTos, result.IsTos);
			Assert.Equal(signupData.TimeZone, result.TimeZone);
		}

		[Fact]
		public async Task Update()
		{
			var service = GetService();
			var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = ""};
			_profileRepo.Setup(p => p.Update(profile)).ReturnsAsync(true);
			await service.Update(profile);
			_profileRepo.Verify(p => p.Update(profile), Times.Once());
		}

		[Fact]
		public async Task UpdateTrimsSig()
		{
			var service = GetService();
			var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = " " };
			var trimProfile = new Profile { Signature = "no"};
			_profileRepo.Setup(p => p.Update(It.IsAny<Profile>())).ReturnsAsync(true).Callback<Profile>(p => trimProfile = p);
			await service.Update(profile);
			Assert.Equal("", trimProfile.Signature);
		}

		[Fact]
		public async Task UpdateThrowsWithNoProfile()
		{
			var service = GetService();
			var profile = new Profile { UserID = 123, Location = "Cleveland", Signature = "" };
			_profileRepo.Setup(p => p.Update(profile)).ReturnsAsync(false);
			await Assert.ThrowsAsync<Exception>(() => service.Update(profile));
			_profileRepo.Verify(p => p.Update(profile), Times.Once());
		}

		[Fact]
		public async Task GetSigsOnlyTakesPostsWithShowSig()
		{
			var posts = new List<Post>
			            	{
								new Post { UserID = 1, ShowSig = false },
								new Post { UserID = 2, ShowSig = true },
								new Post { UserID = 3, ShowSig = false },
								new Post { UserID = 4, ShowSig = true },
								new Post { UserID = 5, ShowSig = true },
								new Post { UserID = 6, ShowSig = false },
			            	};
			var service = GetService();
			var ids = new List<int>();
			_profileRepo.Setup(p => p.GetSignatures(It.IsAny<List<int>>())).Callback<List<int>>(l => ids = l);
			await service.GetSignatures(posts);
			Assert.Equal(3, ids.Count);
			Assert.Equal(2, ids[0]);
			Assert.Equal(4, ids[1]);
			Assert.Equal(5, ids[2]);
		}

		[Fact]
		public async Task GetSigsDoesntSendDupeUserIDs()
		{
			var posts = new List<Post>
			            	{
								new Post { UserID = 1, ShowSig = false },
								new Post { UserID = 2, ShowSig = true },
								new Post { UserID = 2, ShowSig = false },
								new Post { UserID = 2, ShowSig = true },
								new Post { UserID = 3, ShowSig = true },
								new Post { UserID = 3, ShowSig = true },
			            	};
			var service = GetService();
			var ids = new List<int>();
			_profileRepo.Setup(p => p.GetSignatures(It.IsAny<List<int>>())).Callback<List<int>>(l => ids = l);
			await service.GetSignatures(posts);
			Assert.Equal(2, ids.Count);
			Assert.Equal(2, ids[0]);
			Assert.Equal(3, ids[1]);
		}

		[Fact]
		public async Task GetAvatarsDoesntSendDupeUserIDs()
		{
			var posts = new List<Post>
			            	{
								new Post { UserID = 1 },
								new Post { UserID = 2 },
								new Post { UserID = 2 },
								new Post { UserID = 2 },
								new Post { UserID = 3 },
								new Post { UserID = 3 },
			            	};
			var service = GetService();
			var ids = new List<int>();
			_profileRepo.Setup(p => p.GetAvatars(It.IsAny<List<int>>())).Callback<List<int>>(l => ids = l);
			await service.GetAvatars(posts);
			Assert.Equal(3, ids.Count);
			Assert.Equal(1, ids[0]);
			Assert.Equal(2, ids[1]);
			Assert.Equal(3, ids[2]);
		}

		[Fact]
		public async Task UpdatePointsUpdatesPoints()
		{
			var service = GetService();
			var user = new User { UserID = 123 };
			const int total = 87;
			_pointLedger.Setup(x => x.GetPointTotal(user.UserID)).ReturnsAsync(total);
			await service.UpdatePointTotal(user);
			_profileRepo.Verify(x => x.UpdatePoints(user.UserID, total), Times.Once());
		}
	}
}