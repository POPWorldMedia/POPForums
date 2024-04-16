using PopForums.Services.Interfaces;

namespace PopForums.Services;

/// <summary>
/// Used to make sure that incoming names from an external identity provider are unique. In other words, if there's
/// a "John Smith," the next one becomes "John Smith-2."
/// </summary>
public class UserNameReconciler : IUserNameReconciler
{
	private readonly IUserRepository _userRepository;

	public UserNameReconciler(IUserRepository userRepository)
	{
		_userRepository = userRepository;
	}

	public async Task<string> GetUniqueNameForUser(string name)
	{
		var existingMatches = await _userRepository.GetUserNamesThatStartWith(name);
		var uniqueName = name.ToUniqueName(existingMatches.ToList());

		return uniqueName;
	}
}