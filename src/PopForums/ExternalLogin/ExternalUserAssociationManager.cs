using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.ExternalLogin
{
	public interface IExternalUserAssociationManager
	{
		Task<ExternalUserAssociationMatchResult> ExternalUserAssociationCheck(ExternalLoginInfo externalLoginInfo, string ip);
		Task Associate(User user, ExternalLoginInfo externalLoginInfo, string ip);
		Task<List<ExternalUserAssociation>> GetExternalUserAssociations(User user);
		Task RemoveAssociation(User user, int externalUserAssociationID, string ip);
	}

	public class ExternalUserAssociationManager : IExternalUserAssociationManager
	{
		public ExternalUserAssociationManager(IExternalUserAssociationRepository externalUserAssociationRepository, IUserRepository userRepository, ISecurityLogService securityLogService)
		{
			_externalUserAssociationRepository = externalUserAssociationRepository;
			_userRepository = userRepository;
			_securityLogService = securityLogService;
		}

		private readonly IExternalUserAssociationRepository _externalUserAssociationRepository;
		private readonly IUserRepository _userRepository;
		private readonly ISecurityLogService _securityLogService;

		public async Task<ExternalUserAssociationMatchResult> ExternalUserAssociationCheck(ExternalLoginInfo externalLoginInfo, string ip)
		{
			if (externalLoginInfo == null)
				throw new ArgumentNullException(nameof(externalLoginInfo));
			var match = await _externalUserAssociationRepository.Get(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey);
			if (match == null)
			{
				await _securityLogService.CreateLogEntry((int?)null, null, ip, $"Issuer: {externalLoginInfo.LoginProvider}, Provider: {externalLoginInfo.ProviderKey}, Name: {externalLoginInfo.ProviderDisplayName}", SecurityLogType.ExternalAssociationCheckFailed);
				return new ExternalUserAssociationMatchResult {Successful = false};
			}
			var user = await _userRepository.GetUser(match.UserID);
			if (user == null)
			{
				await _securityLogService.CreateLogEntry((int?)null, null, ip, $"Issuer: {externalLoginInfo.LoginProvider}, Provider: {externalLoginInfo.ProviderKey}, Name: {externalLoginInfo.ProviderDisplayName}", SecurityLogType.ExternalAssociationCheckFailed);
				return new ExternalUserAssociationMatchResult {Successful = false};
			}
			var result = new ExternalUserAssociationMatchResult
			             {
				             Successful = true,
				             ExternalUserAssociation = match,
				             User = user
			             };
			await _securityLogService.CreateLogEntry(user, user, ip, $"Issuer: {match.Issuer}, Provider: {match.ProviderKey}, Name: {match.Name}", SecurityLogType.ExternalAssociationCheckSuccessful);
			return result;
		}

		public async Task Associate(User user, ExternalLoginInfo externalLoginInfo, string ip)
		{
			if (user == null)
				throw new ArgumentNullException(nameof(user));
			if (externalLoginInfo != null)
			{
				if (string.IsNullOrEmpty(externalLoginInfo.LoginProvider))
					throw new NullReferenceException("The external login info contains no provider.");
				if (string.IsNullOrEmpty(externalLoginInfo.ProviderKey))
					throw new NullReferenceException("The external login info contains no provider key.");
				if (string.IsNullOrEmpty(externalLoginInfo.ProviderDisplayName))
					externalLoginInfo.ProviderDisplayName = string.Empty;
				await _externalUserAssociationRepository.Save(user.UserID, externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey, externalLoginInfo.ProviderDisplayName);
				await _securityLogService.CreateLogEntry(user, user, ip, $"Provider: {externalLoginInfo.LoginProvider}, DisplayName: {externalLoginInfo.ProviderDisplayName}", SecurityLogType.ExternalAssociationSet);
			}
		}

		public async Task<List<ExternalUserAssociation>> GetExternalUserAssociations(User user)
		{
			return await _externalUserAssociationRepository.GetByUser(user.UserID);
		}

		public async Task RemoveAssociation(User user, int externalUserAssociationID, string ip)
		{
			var association = await _externalUserAssociationRepository.Get(externalUserAssociationID);
			if (association == null)
				return;
			if (association.UserID != user.UserID)
				throw new Exception($"Can't delete external user association {externalUserAssociationID} because it doesn't match UserID {user.UserID}.");
			await _externalUserAssociationRepository.Delete(externalUserAssociationID);
			await _securityLogService.CreateLogEntry(user, user, ip, $"Issuer: {association.Issuer}, Provider: {association.ProviderKey}, Name: {association.Name}", SecurityLogType.ExternalAssociationRemoved);
		}
	}
}