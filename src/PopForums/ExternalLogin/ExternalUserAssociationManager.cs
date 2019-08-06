using System;
using System.Collections.Generic;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.ExternalLogin
{
	public interface IExternalUserAssociationManager
	{
		ExternalUserAssociationMatchResult ExternalUserAssociationCheck(ExternalLoginInfo externalLoginInfo, string ip);
		void Associate(User user, ExternalLoginInfo externalLoginInfo, string ip);
		List<ExternalUserAssociation> GetExternalUserAssociations(User user);
		void RemoveAssociation(User user, int externalUserAssociationID, string ip);
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

		public ExternalUserAssociationMatchResult ExternalUserAssociationCheck(ExternalLoginInfo externalLoginInfo, string ip)
		{
			if (externalLoginInfo == null)
				throw new ArgumentNullException(nameof(externalLoginInfo));
			var match = _externalUserAssociationRepository.Get(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey);
			if (match == null)
			{
				_securityLogService.CreateLogEntry((int?)null, null, ip, $"Issuer: {externalLoginInfo.LoginProvider}, Provider: {externalLoginInfo.ProviderKey}, Name: {externalLoginInfo.ProviderDisplayName}", SecurityLogType.ExternalAssociationCheckFailed);
				return new ExternalUserAssociationMatchResult {Successful = false};
			}
			var user = _userRepository.GetUser(match.UserID);
			if (user == null)
			{
				_securityLogService.CreateLogEntry((int?)null, null, ip, $"Issuer: {externalLoginInfo.LoginProvider}, Provider: {externalLoginInfo.ProviderKey}, Name: {externalLoginInfo.ProviderDisplayName}", SecurityLogType.ExternalAssociationCheckFailed);
				return new ExternalUserAssociationMatchResult {Successful = false};
			}
			var result = new ExternalUserAssociationMatchResult
			             {
				             Successful = true,
				             ExternalUserAssociation = match,
				             User = user
			             };
			_securityLogService.CreateLogEntry(user, user, ip, $"Issuer: {match.Issuer}, Provider: {match.ProviderKey}, Name: {match.Name}", SecurityLogType.ExternalAssociationCheckSuccessful);
			return result;
		}

		public void Associate(User user, ExternalLoginInfo externalLoginInfo, string ip)
		{
			if (user == null)
				throw new ArgumentNullException(nameof(user));
			if (externalLoginInfo != null)
			{
				if (string.IsNullOrEmpty(externalLoginInfo.LoginProvider))
					throw new NullReferenceException("The external login info contains no provider.");
				if (string.IsNullOrEmpty(externalLoginInfo.ProviderKey))
					throw new NullReferenceException("The external login info contains no provider key.");
				_externalUserAssociationRepository.Save(user.UserID, externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey, externalLoginInfo.ProviderDisplayName);
				_securityLogService.CreateLogEntry(user, user, ip, $"Provider: {externalLoginInfo.LoginProvider}, DisplayName: {externalLoginInfo.ProviderDisplayName}", SecurityLogType.ExternalAssociationSet);
			}
		}

		public List<ExternalUserAssociation> GetExternalUserAssociations(User user)
		{
			return _externalUserAssociationRepository.GetByUser(user.UserID);
		}

		public void RemoveAssociation(User user, int externalUserAssociationID, string ip)
		{
			var association = _externalUserAssociationRepository.Get(externalUserAssociationID);
			if (association == null)
				return;
			if (association.UserID != user.UserID)
				throw new Exception($"Can't delete external user association {externalUserAssociationID} because it doesn't match UserID {user.UserID}.");
			_externalUserAssociationRepository.Delete(externalUserAssociationID);
			_securityLogService.CreateLogEntry(user, user, ip, $"Issuer: {association.Issuer}, Provider: {association.ProviderKey}, Name: {association.Name}", SecurityLogType.ExternalAssociationRemoved);
		}
	}
}