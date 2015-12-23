namespace PopForums.Models
{
	public class SetupVariables
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string ForumTitle { get; set; }
		public int ServerTimeZone { get; set; }
		public bool ServerDaylightSaving { get; set; }
		public string SmtpServer { get; set; }
		public int SmtpPort { get; set; }
		public string MailerAddress { get; set; }
		public bool UseEsmtp { get; set; }
		public string SmtpUser { get; set; }
		public string SmtpPassword { get; set; }
		public bool UseSslSmtp { get; set; }
	}
}
