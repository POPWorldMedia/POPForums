﻿using PopForums.Services.Interfaces;

namespace PopForums.ScoringGame;

public interface IUserAwardService
{
	Task IssueAward(User user, AwardDefinition awardDefinition);
	Task<bool> IsAwarded(User user, AwardDefinition awardDefinition);
	Task<List<UserAward>> GetAwards(User user);
}

public class UserAwardService : IUserAwardService
{
	public UserAwardService(IUserAwardRepository userAwardRepository, INotificationTunnel notificationTunnel, ITenantService tenantService)
	{
		_userAwardRepository = userAwardRepository;
		_notificationTunnel = notificationTunnel;
		_tenantService = tenantService;
	}

	private readonly IUserAwardRepository _userAwardRepository;
	private readonly INotificationTunnel _notificationTunnel;
	private readonly ITenantService _tenantService;

	public async Task IssueAward(User user, AwardDefinition awardDefinition)
	{
		await _userAwardRepository.IssueAward(user.UserID, awardDefinition.AwardDefinitionID, awardDefinition.Title, awardDefinition.Description, DateTime.UtcNow);
		var tenantID = _tenantService.GetTenant();
		_notificationTunnel.SendNotificationForUserAward(awardDefinition.Title, user.UserID, tenantID);
	}

	public async Task<bool> IsAwarded(User user, AwardDefinition awardDefinition)
	{
		return await _userAwardRepository.IsAwarded(user.UserID, awardDefinition.AwardDefinitionID);
	}

	public async Task<List<UserAward>> GetAwards(User user)
	{
		return await _userAwardRepository.GetAwards(user.UserID);
	}
}