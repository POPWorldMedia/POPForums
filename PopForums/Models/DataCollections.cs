using System.Collections.Generic;

namespace PopForums.Models
{
	public class DataCollections
	{
		public static Dictionary<int, string> TimeZones()
		{
			var timeZones = new Dictionary<int, string>();
			timeZones.Add(-11, "-11 Samoa");
			timeZones.Add(-10, "-10 Hawaii");
			timeZones.Add(-9, "-9 Alaska");
			timeZones.Add(-8, "-8 Pacific");
			timeZones.Add(-7, "-7 Mountain");
			timeZones.Add(-6, "-6 Central");
			timeZones.Add(-5, "-5 Eastern");
			timeZones.Add(-4, "-4 Atlantic");
			timeZones.Add(-3, "-3 Brasilia");
			timeZones.Add(-2, "-2 Mid-Atlantic");
			timeZones.Add(-1, "-1 Azores");
			timeZones.Add(0, "UTC/GMT");
			timeZones.Add(1, "+1 Berlin, Rome, Stockholm");
			timeZones.Add(2, "+2 Athen, Cairo, Jerusalem");
			timeZones.Add(3, "+3 Baghdad, Moscow, Kuwait City");
			timeZones.Add(4, "+4 Abu Dhabi, Muscat");
			timeZones.Add(5, "+5 Islamabad, New Delhi");
			timeZones.Add(6, "+6 Astana, Dhaka");
			timeZones.Add(7, "+7 Bangkok");
			timeZones.Add(8, "+8 Singapore, Taipei");
			timeZones.Add(9, "+9 Tokyo, Seoul");
			timeZones.Add(10, "+10 Sydney, Guam");
			timeZones.Add(11, "+11 Soloman Islands");
			timeZones.Add(12, "+12 Fiji, Marshall Islands");
			return timeZones;
		}
	}
}
