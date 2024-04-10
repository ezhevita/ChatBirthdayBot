using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ChatBirthdayBot;

public static class Utilities
{
	private static readonly Dictionary<CultureInfo, string> _cachedLongDatePatterns = new()
	{
		{CultureInfo.InvariantCulture, "dd MMMM yyyy"}
	};

	public static string ToLongDateStringWithoutDayOfWeek(this DateTime d)
	{
		var currentCulture = CultureInfo.CurrentCulture;
		if (_cachedLongDatePatterns.TryGetValue(currentCulture, out var pattern))
		{
			return d.ToString(pattern, CultureInfo.CurrentCulture);
		}

		pattern = CultureInfo.CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('D')
			.FirstOrDefault(
				dateTimePattern => !dateTimePattern.Contains("ddd", StringComparison.Ordinal) &&
					!dateTimePattern.Contains("dddd", StringComparison.Ordinal)) ?? "D";

		lock (_cachedLongDatePatterns)
		{
			_cachedLongDatePatterns[currentCulture] = pattern;
		}

		return d.ToString(pattern, CultureInfo.CurrentCulture);
	}
}
