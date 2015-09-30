namespace PopForums.ExternalLogin
{
	public class ExternalAuthenticationResult
	{
		public string Issuer { get; set; }
		public string ProviderKey { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
	}
}