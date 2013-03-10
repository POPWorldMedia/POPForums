using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace PopForums.Models
{
	public class User : IPrincipal
	{
		public User(int userID, DateTime creationDate)
		{
			UserID = userID;
			CreationDate = creationDate;
		}

		public int UserID { get; private set; }
		public DateTime CreationDate { get; private set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public DateTime LastActivityDate { get; set; }
		public DateTime LastLoginDate { get; set; }
		public Guid AuthorizationKey { get; set; }
		public bool IsApproved { get; set; }
		public List<string> Roles { get; set; }

		public bool IsInRole(string role)
		{
			if (Roles == null)
				throw new Exception("Roles not set for user.");
			return Roles.Contains(role);
		}

		public IIdentity Identity
		{
			get
			{
				return new GenericIdentity(Name);
			}
		}
	}
}