using System;
using System.Collections.Generic;
using PopForums.Models;
using PopForums.Repositories;
using PopForums.Services;

namespace PopForums.ExternalLogin
{
	public interface IExternalUserAssociationManager
	{
		ExternalUserAssociationMatchResult ExternalUserAssociationCheck(ExternalAuthenticationResult externalAuthenticationResult, string ip);
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

		public ExternalUserAssociationMatchResult ExternalUserAssociationCheck(ExternalAuthenticationResult externalAuthenticationResult, string ip)
		{
			if (externalAuthenticationResult == null)
				throw new ArgumentNullException(nameof(externalAuthenticationResult));
			var match = _externalUserAssociationRepository.Get(externalAuthenticationResult.Issuer, externalAuthenticationResult.ProviderKey);
			if (match == null)
			{
				_securityLogService.CreateLogEntry((int?)null, null, ip, String.Format("Issuer: {0}, Provider: {1}, Name: {2}", externalAuthenticationResult.Issuer, externalAuthenticationResult.ProviderKey, externalAuthenticationResult.Name), SecurityLogType.ExternalAssociationCheckFailed);
				return new ExternalUserAssociationMatchResult {Successful = false};
			}
			var user = _userRepository.GetUser(match.UserID);
			if (user == null)
			{
				_securityLogService.CreateLogEntry((int?)null, null, ip, String.Format("Issuer: {0}, Provider: {1}, Name: {2}", externalAuthenticationResult.Issuer, externalAuthenticationResult.ProviderKey, externalAuthenticationResult.Name), SecurityLogType.ExternalAssociationCheckFailed);
				return new ExternalUserAssociationMatchResult {Successful = false};
			}
			var result = new ExternalUserAssociationMatchResult
			             {
				             Successful = true,
				             ExternalUserAssociation = match,
				             User = user
			             };
			_securityLogService.CreateLogEntry(user, user, ip, String.Format("Issuer: {0}, Provider: {1}, Name: {2}", match.Issuer, match.ProviderKey, match.Name), SecurityLogType.ExternalAssociationCheckSuccessful);
			return result;
		}

		public void Associate(User user, ExternalLoginInfo externalLoginInfo, string ip)
		{
			if (user == null)
				throw new ArgumentNullException("user");
			if (externalLoginInfo != null)
			{
				if (String.IsNullOrEmpty(externalLoginInfo.LoginProvider))
					throw new NullReferenceException("The external login info contains no provider.");
				if (String.IsNullOrEmpty(externalLoginInfo.ProviderKey))
					throw new NullReferenceException("The external login info contains no provider key.");
				_externalUserAssociationRepository.Save(user.UserID, externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey, externalLoginInfo.ProviderDisplayName);
				_securityLogService.CreateLogEntry(user, user, ip, String.Format("Provider: {0}, DisplayName: {1}", externalLoginInfo.LoginProvider, externalLoginInfo.ProviderDisplayName), SecurityLogType.ExternalAssociationSet);
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
				throw new Exception(String.Format("Can't delete external user association {0} because it doesn't match UserID {1}.", externalUserAssociationID, user.UserID));
			_externalUserAssociationRepository.Delete(externalUserAssociationID);
			_securityLogService.CreateLogEntry(user, user, ip, String.Format("Issuer: {0}, Provider: {1}, Name: {2}", association.Issuer, association.ProviderKey, association.Name), SecurityLogType.ExternalAssociationRemoved);
		}
	}
}