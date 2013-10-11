using System.Collections.Generic;
using Microsoft.Owin.Security;
using PopForums.ExternalLogin;

namespace PopForums.Models
{
	public class ExternalLoginContainer
	{
		public List<ExternalUserAssociation> ExternalUserAssociations { get; set; }
		public List<AuthenticationDescription> AuthenticationDescriptions { get; set; } 
	}
}