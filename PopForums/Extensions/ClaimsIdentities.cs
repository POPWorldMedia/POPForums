using System;
using System.Security.Claims;

namespace PopForums.Extensions
{
	public static class ClaimsIdentities
	{
		public static string FindFirstValue(this ClaimsIdentity identity, string claimType)
		{
			if (identity == null)
				throw new ArgumentNullException("identity");
			var first = identity.FindFirst(claimType);
			if (first != null)
				return first.Value;
			return null;
		}
	}
}