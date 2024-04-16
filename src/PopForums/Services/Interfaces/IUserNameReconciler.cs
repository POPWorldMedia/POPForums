namespace PopForums.Services.Interfaces;

public interface IUserNameReconciler
{
    Task<string> GetUniqueNameForUser(string name);
}
