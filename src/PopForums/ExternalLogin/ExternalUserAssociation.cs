namespace PopForums.ExternalLogin
{
	public class ExternalUserAssociation
	{
		public int ExternalUserAssociationID { get; set; }
		public int UserID { get; set; }
		public string Issuer { get; set; }
		public string ProviderKey { get; set; }
		public string Name { get; set; }
	}
}