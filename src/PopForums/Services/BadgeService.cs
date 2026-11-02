namespace PopForums.Services;

public interface IBadgeService
{
	Task<List<Badge>> GetAll();
	Task<Badge> Get(int badgeID);
	Task<BasicServiceResponse<Badge>> Save(Badge badge);
	Task Delete(int badgeID);
	Task<Dictionary<int, List<Badge>>> GetBadges(List<int> userIDs);
	Task<Dictionary<int, List<Badge>>> GetBadges(List<Post> posts);
	Task<List<Badge>> GetBadges(int userID);
}

public class BadgeService(IBadgeRepository badgeRepository, IUserService userService) : IBadgeService
{
	private static readonly Regex ColorPattern = new("^[0-9a-fA-F]{6}$");

	public async Task<List<Badge>> GetAll()
	{
		return await badgeRepository.GetAll();
	}

	public async Task<Badge> Get(int badgeID)
	{
		return await badgeRepository.Get(badgeID);
	}

	public async Task<BasicServiceResponse<Badge>> Save(Badge badge)
	{
		if (string.IsNullOrWhiteSpace(badge.Name))
			return BasicServiceResponse<Badge>.Failed("Badge name is required.");
		if (string.IsNullOrWhiteSpace(badge.Role))
			return BasicServiceResponse<Badge>.Failed("Badge role is required.");
		if (badge.ForegroundColor == null || !ColorPattern.IsMatch(badge.ForegroundColor))
			return BasicServiceResponse<Badge>.Failed("Foreground color must be six hex digits, like ffffff.");
		if (badge.BackgroundColor == null || !ColorPattern.IsMatch(badge.BackgroundColor))
			return BasicServiceResponse<Badge>.Failed("Background color must be six hex digits, like ffffff.");
		badge.ForegroundColor = badge.ForegroundColor.ToLower();
		badge.BackgroundColor = badge.BackgroundColor.ToLower();
		if (badge.BadgeID == 0)
			badge.BadgeID = await badgeRepository.Create(badge);
		else
			await badgeRepository.Update(badge);
		return BasicServiceResponse<Badge>.Success(badge);
	}

	public async Task Delete(int badgeID)
	{
		await badgeRepository.Delete(badgeID);
	}

	public async Task<Dictionary<int, List<Badge>>> GetBadges(List<int> userIDs)
	{
		var effectiveRoles = await userService.GetEffectiveRoles(userIDs);
		var badgesByRole = (await badgeRepository.GetAll()).ToLookup(b => b.Role);
		var result = new Dictionary<int, List<Badge>>();
		foreach (var (userID, roles) in effectiveRoles)
		{
			var badges = roles.Distinct().SelectMany(role => badgesByRole[role]).ToList();
			if (badges.Count > 0)
				result.Add(userID, badges);
		}
		return result;
	}

	public async Task<Dictionary<int, List<Badge>>> GetBadges(List<Post> posts)
	{
		var userIDs = posts.Select(p => p.UserID).Distinct().ToList();
		return await GetBadges(userIDs);
	}

	public async Task<List<Badge>> GetBadges(int userID)
	{
		var badges = await GetBadges(new List<int> { userID });
		return badges.TryGetValue(userID, out var list) ? list : new List<Badge>();
	}
}
