namespace PopForums.Repositories;

public interface IRenewalEnqueueClaimRepository
{
	Task<bool> TryClaim(DateOnly date);
}
