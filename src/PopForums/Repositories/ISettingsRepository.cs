namespace PopForums.Repositories;

public interface ISettingsRepository
{
	Dictionary<string, string> Get();
	void Save(Dictionary<string, object> dictionary);
	event Action OnSettingsInvalidated;
}