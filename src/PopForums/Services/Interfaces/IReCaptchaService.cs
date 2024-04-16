namespace PopForums.Services.Interfaces;

public interface IReCaptchaService
{
    Task<ReCaptchaResponse> VerifyToken(string token, string ip);
}
