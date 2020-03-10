using System;
using System.Collections.Generic;
using System.Text;

namespace PopForums.Extensions
{
	public static class DateTimes
	{
		public static string AsUtc8601(this DateTime dt)
		{
			return dt.Kind switch
			{
				DateTimeKind.Utc => dt.ToString("o"),
				DateTimeKind.Unspecified => dt.ToString("o") + 'Z',
				_ => throw new ArgumentException("Date must NOT be DateTimeKind.local"),
			};
		}
	}
}
