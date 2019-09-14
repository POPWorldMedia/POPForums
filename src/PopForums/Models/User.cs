using System;
using System.Collections.Generic;

namespace PopForums.Models
{
	public class User
	{
		public int UserID { get; set; }
		public DateTime CreationDate { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public Guid AuthorizationKey { get; set; }
		public bool IsApproved { get; set; }
		public List<string> Roles { get; set; }

		public bool IsInRole(string role)
		{
			if (Roles == null)
				throw new Exception("Roles not set for user.");
			return Roles.Contains(role);
		}
	}
}