using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using ChatBirthdayBot.Localization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot.Commands;

public class BirthdayInfoCommand : ICommand
{
	private readonly DataContext _context;

	public BirthdayInfoCommand(DataContext context)
	{
		_context = context;
	}

	public virtual string CommandName => "birthday";
	public bool ShouldBeExecutedForChatType(ChatType chatType) => chatType == ChatType.Private;

	public async Task<string?> ExecuteCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
	{
		var currentUser = await _context.Users.FindAsync(new object[] {message.From!.Id}, cancellationToken);

		if (currentUser is not {BirthdayDay: not null, BirthdayMonth: not null})
			return Lines.BirthdayNotSet;

		DateTime date = new(currentUser.BirthdayYear ?? 0004, currentUser.BirthdayMonth.Value, currentUser.BirthdayDay.Value);

		return string.Format(
			CultureInfo.CurrentCulture, Lines.BirthdayDate, date.Year == 0004
				? date.ToString("M", CultureInfo.CurrentCulture)
				: date.ToLongDateString().Replace(
					CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(date.DayOfWeek), "", StringComparison.Ordinal
				).TrimStart(',', ' ').TrimEnd('.')
		);
	}
}
