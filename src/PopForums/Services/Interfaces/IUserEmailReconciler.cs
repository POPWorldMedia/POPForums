namespace PopForums.Services.Interfaces;

public interface IUserEmailReconciler
{
    Task<string> GetUniqueEmail(string email, string externalID);
}
