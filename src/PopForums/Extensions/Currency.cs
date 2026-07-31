using System.Collections.Concurrent;
using System.Globalization;

namespace PopForums.Extensions;

public static class Currency
{
	private static readonly ConcurrentDictionary<string, CultureInfo> _cultureCache = new();

	public static string ToCurrencyString(this decimal amount, string isoCurrencyCode)
	{
		var culture = GetCultureForCurrency(isoCurrencyCode);
		return amount.ToString("C2", culture);
	}

	private static CultureInfo GetCultureForCurrency(string isoCurrencyCode)
	{
		if (string.IsNullOrWhiteSpace(isoCurrencyCode))
			return CultureInfo.InvariantCulture;
		var code = isoCurrencyCode.ToUpperInvariant();
		return _cultureCache.GetOrAdd(code, FindCultureForCurrency);
	}

	private static CultureInfo FindCultureForCurrency(string isoCurrencyCode)
	{
		foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
		{
			try
			{
				var region = new RegionInfo(culture.Name);
				if (region.ISOCurrencySymbol.Equals(isoCurrencyCode, StringComparison.OrdinalIgnoreCase))
					return culture;
			}
			catch (Exception)
			{
				// some cultures don't resolve to a valid region; skip them
			}
		}
		return CultureInfo.InvariantCulture;
	}
}
