namespace PopForums.Services.Interfaces;

public interface IProfileService
{
    Task<Profile> GetProfile(User user);

    Task Create(Profile profile);

    Task Update(Profile profile);

    Task<Profile> GetProfileForEdit(User user, bool forcePlainText = false);

    Task EditUserProfile(User user, UserEditProfile userEditProfile);

    Task<Dictionary<int, string>> GetSignatures(List<Post> posts);

    Task<Dictionary<int, int>> GetAvatars(List<Post> posts);

    Task SetCurrentImageIDToNull(int userID);

    string GetUnsubscribeHash(User user);

    Task<bool> Unsubscribe(User user, string hash);

    Task UpdatePointTotal(User user);
}
