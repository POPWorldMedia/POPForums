using PopForums.Services.Interfaces;

namespace PopForums.Services;

/// <summary>
/// Checks for existing email addresses from an external identity provider. If a match is found, it mangles the address
/// to use an "example.com" address, which is not real per IETF RFC 2606.
/// </summary>
public class UserEmailReconciler : IUserEmailReconciler
{
	private readonly IUserRepository _userRepository;

	public UserEmailReconciler(IUserRepository userRepository)
	{
		_userRepository = userRepository;
	}

	public async Task<string> GetUniqueEmail(string email, string externalID)
	{
		var match = await _userRepository.GetUserByEmail(email);

		if (match is null)
        {
            return email;
        }

        var uniqueEmail = $"{email.Replace("@","-at-")}@{externalID}.example.com";

		return uniqueEmail;
	}
}