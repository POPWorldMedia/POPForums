namespace PopForums.Services.Interfaces;

public interface IUserRetrievalShim
{
    User GetUser();

    Profile GetProfile();
}