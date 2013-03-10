using System;
using Moq;
using NUnit.Framework;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;
using System.Collections.Generic;

namespace PopForums.Test.Services
{
	[TestFixture]
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

		[Test]
		public void GetProfile()
		{
			var service = GetService();
			var profile = new Profile(123) { Aim = "blah", Location = "Cleveland" };
			var user = UserServiceTests.GetDummyUser("Jeff", "a@b.com");
			_profileRepo.Setup(p => p.GetProfile(user.UserID)).Returns(profile);
			var result = service.GetProfile(user);
			Assert.AreEqual(profile, result);
			_profileRepo.Verify(p => p.GetProfile(user.UserID), Times.Once());
		}

		[Test]
		public void GetProfileReturnsNullForNullUser()
		{
			var service = GetService();
			var result = service.GetProfile(null);
			Assert.IsNull(result);
		}

		[Test]
		public void GetProfileForEditParsesSig()
		{
			var service = GetService();
			var profile = new Profile(123) { Aim = "blah", Location = "Cleveland", Signature = "blah" };
			var user = UserServiceTests.GetDummyUser("Jeff", "a@b.com");
			_profileRepo.Setup(p => p.GetProfile(user.UserID)).Returns(profile);
			_textParsingService.Setup(t => t.ClientHtmlToForumCode("blah")).Returns("parsed");
			var result = service.GetProfileForEdit(user);
			Assert.AreEqual("parsed", result.Signature);
			_profileRepo.Verify(p => p.GetProfile(user.UserID), Times.Once());
		}

		[Test]
		public void CreateFromProfileObject()
		{
			var service = GetService();
			var profile = new Profile(123) { Aim = "blah", Location = "Cleveland" };
			_profileRepo.Setup(p => p.Create(profile));
			service.Create(profile);
			_profileRepo.Verify(p => p.Create(profile), Times.Once());
		}

		[Test]
		public void CreateFromProfileThrowsWithoutUserID()
		{
			var service = GetService();
			var profile = new Profile();
			Assert.Throws<Exception>(() => service.Create(profile));
			_profileRepo.Verify(p => p.Create(profile), Times.Never());
		}

		[Test]
		public void CreateFromUserAndSignupData()
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
			var result = service.Create(user, signupData);
			Assert.AreEqual(user.UserID, result.UserID);
			Assert.AreEqual(signupData.IsDaylightSaving, result.IsDaylightSaving);
			Assert.AreEqual(signupData.IsSubscribed, result.IsSubscribed);
			Assert.AreEqual(signupData.IsTos, result.IsTos);
			Assert.AreEqual(signupData.TimeZone, result.TimeZone);
		}

		[Test]
		public void Update()
		{
			var service = GetService();
			var profile = new Profile(123) {Aim = "blah", Location = "Cleveland", Signature = ""};
			_profileRepo.Setup(p => p.Update(profile)).Returns(true);
			service.Update(profile);
			_profileRepo.Verify(p => p.Update(profile), Times.Once());
		}

		[Test]
		public void UpdateTrimsSig()
		{
			var service = GetService();
			var profile = new Profile(123) { Aim = "blah", Location = "Cleveland", Signature = " " };
			var trimProfile = new Profile { Signature = "no"};
			_profileRepo.Setup(p => p.Update(It.IsAny<Profile>())).Returns(true).Callback<Profile>(p => trimProfile = p);
			service.Update(profile);
			Assert.AreEqual("", trimProfile.Signature);
		}

		[Test]
		public void UpdateThrowsWithNoProfile()
		{
			var service = GetService();
			var profile = new Profile(123) { Aim = "blah", Location = "Cleveland", Signature = "" };
			_profileRepo.Setup(p => p.Update(profile)).Returns(false);
			Assert.Throws<Exception>(() => service.Update(profile));
			_profileRepo.Verify(p => p.Update(profile), Times.Once());
		}

		[Test]
		public void GetSigsOnlyTakesPostsWithShowSig()
		{
			var posts = new List<Post>
			            	{
								new Post(1) { UserID = 1, ShowSig = false },
								new Post(1) { UserID = 2, ShowSig = true },
								new Post(1) { UserID = 3, ShowSig = false },
								new Post(1) { UserID = 4, ShowSig = true },
								new Post(1) { UserID = 5, ShowSig = true },
								new Post(1) { UserID = 6, ShowSig = false },
			            	};
			var service = GetService();
			var ids = new List<int>();
			_profileRepo.Setup(p => p.GetSignatures(It.IsAny<List<int>>())).Callback<List<int>>(l => ids = l);
			service.GetSignatures(posts);
			Assert.AreEqual(3, ids.Count);
			Assert.AreEqual(ids[0], 2);
			Assert.AreEqual(ids[1], 4);
			Assert.AreEqual(ids[2], 5);
		}

		[Test]
		public void GetSigsDoesntSendDupeUserIDs()
		{
			var posts = new List<Post>
			            	{
								new Post(1) { UserID = 1, ShowSig = false },
								new Post(1) { UserID = 2, ShowSig = true },
								new Post(1) { UserID = 2, ShowSig = false },
								new Post(1) { UserID = 2, ShowSig = true },
								new Post(1) { UserID = 3, ShowSig = true },
								new Post(1) { UserID = 3, ShowSig = true },
			            	};
			var service = GetService();
			var ids = new List<int>();
			_profileRepo.Setup(p => p.GetSignatures(It.IsAny<List<int>>())).Callback<List<int>>(l => ids = l);
			service.GetSignatures(posts);
			Assert.AreEqual(2, ids.Count);
			Assert.AreEqual(ids[0], 2);
			Assert.AreEqual(ids[1], 3);
		}

		[Test]
		public void GetAvatarsDoesntSendDupeUserIDs()
		{
			var posts = new List<Post>
			            	{
								new Post(1) { UserID = 1 },
								new Post(1) { UserID = 2 },
								new Post(1) { UserID = 2 },
								new Post(1) { UserID = 2 },
								new Post(1) { UserID = 3 },
								new Post(1) { UserID = 3 },
			            	};
			var service = GetService();
			var ids = new List<int>();
			_profileRepo.Setup(p => p.GetAvatars(It.IsAny<List<int>>())).Callback<List<int>>(l => ids = l);
			service.GetAvatars(posts);
			Assert.AreEqual(3, ids.Count);
			Assert.AreEqual(ids[0], 1);
			Assert.AreEqual(ids[1], 2);
			Assert.AreEqual(ids[2], 3);
		}

		[Test]
		public void UpdatePointsUpdatesPoints()
		{
			var service = GetService();
			var user = new User(123, DateTime.MinValue);
			const int total = 87;
			_pointLedger.Setup(x => x.GetPointTotal(user.UserID)).Returns(total);
			service.UpdatePointTotal(user);
			_profileRepo.Verify(x => x.UpdatePoints(user.UserID, total), Times.Once());
		}
	}
}