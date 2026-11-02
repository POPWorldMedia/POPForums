namespace PopForums.Repositories;

public interface IBadgeRepository
{
	Task<List<Badge>> GetAll();
	Task<Badge> Get(int badgeID);
	Task<int> Create(Badge badge);
	Task Update(Badge badge);
	Task Delete(int badgeID);
}
