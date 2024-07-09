using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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

		string? pattern;
		lock (_cachedLongDatePatterns)
		{
			if (!_cachedLongDatePatterns.TryGetValue(currentCulture, out pattern))
			{
				pattern = CultureInfo.CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('D')
					.FirstOrDefault(
						dateTimePattern => !dateTimePattern.Contains("ddd", StringComparison.Ordinal) &&
							!dateTimePattern.Contains("dddd", StringComparison.Ordinal)) ?? "D";

				_cachedLongDatePatterns[currentCulture] = pattern;
			}
		}

		return d.ToString(pattern, CultureInfo.CurrentCulture);
	}

	public static BotCommandScope ChatTypeToCommandScope(ChatType chatType)
	{
		return chatType switch
		{
			ChatType.Private => new BotCommandScopeAllPrivateChats(),
			ChatType.Group or ChatType.Supergroup => new BotCommandScopeAllGroupChats(),
			_ => throw new ArgumentOutOfRangeException(nameof(chatType))
		};
	}
}
