namespace PopForums.Services.Interfaces;

public interface IImageService
{
    Task<bool> IsUserImageApproved(int userImageID);

    [Obsolete("Use GetAvatarImageStream(int) instead.")]
    Task<byte[]> GetAvatarImageData(int userAvatarID);

    Task<IStreamResponse> GetAvatarImageStream(int userAvatarID);

    [Obsolete("Use GetUserImageStream(int) instead.")]
    Task<byte[]> GetUserImageData(int userImageID);

    Task<IStreamResponse> GetUserImageStream(int userImageID);

    Task<DateTime> GetAvatarImageLastModification(int userAvatarID);

    Task<DateTime> GetUserImageLastModifcation(int userImageID);

    byte[] ConstrainResize(byte[] bytes, int maxWidth, int maxHeight, int qualityLevel, bool cropInsteadOfConstrain);

    Task<List<UserImage>> GetUnapprovedUserImages();

    Task ApproveUserImage(int userImageID);

    Task DeleteUserImage(int userImageID);

    Task<UserImage> GetUserImage(int userImageID);

    Task<UserImageApprovalContainer> GetUnapprovedUserImageContainer();
}
