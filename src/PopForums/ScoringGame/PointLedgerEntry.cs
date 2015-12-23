using System;

namespace PopForums.ScoringGame
{
	public class PointLedgerEntry
	{
		public int UserID { get; set; }
		public string EventDefinitionID { get; set; }
		public int Points { get; set; }
		public DateTime TimeStamp { get; set; }
	}
}
