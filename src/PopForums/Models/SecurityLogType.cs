namespace PopForums.Models
{
	public enum SecurityLogType
	{
		Undefined = 0,
		Login = 1,
		Logout = 2,
		PasswordChange = 3,
		EmailChange = 4,
		FailedLogin = 5,
		UserCreated = 6,
		UserDeleted = 7,
		RoleCreated = 8,
		RoleDeleted = 9,
		UserAddedToRole = 10,
		UserRemovedFromRole = 11,
		UserSessionStart = 12,
		UserSessionEnd = 13,
		NameChange = 14,
		IsApproved = 15,
		IsNotApproved = 16,
		ExternalAssociationSet = 17,
		ExternalAssociationRemoved = 18,
		ExternalAssociationCheckSuccessful = 19,
		ExternalAssociationCheckFailed = 20,
		ReCaptchaFailed = 21
	}
}