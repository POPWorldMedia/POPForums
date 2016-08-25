using System;

namespace PopForums.ScoringGame
{
	public class UserAward
	{
		public int UserAwardID { get; set; }
		public int UserID { get; set; }
		public string AwardDefinitionID { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime TimeStamp { get; set; }
	}
}
