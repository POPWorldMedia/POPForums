using System.Collections.Generic;
using PopForums.Models;

namespace PopForums.Services
{
	public interface IProfileService
	{
		Profile GetProfile(User user);
		void Create(Profile profile);
		Profile Create(User user, SignupData signupData);
		void Update(Profile profile);
		Profile GetProfileForEdit(User user);
		Dictionary<int, string> GetSignatures(List<Post> posts);
		Dictionary<int, int> GetAvatars(List<Post> posts);
		void SetCurrentImageIDToNull(int userID);
		string GetUnsubscribeHash(User user);
		bool Unsubscribe(User user, string hash);
		void UpdatePointTotal(User user);
	}
}