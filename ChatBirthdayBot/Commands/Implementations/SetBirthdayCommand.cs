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

public class SetBirthdayCommand : ICommand
{
	private readonly DataContext _context;

	public SetBirthdayCommand(DataContext context)
	{
		_context = context;
	}

	public string CommandName => "setbirthday";

	public bool ShouldBeExecutedForChatType(ChatType chatType) => chatType is ChatType.Private;

	public async Task<string?> ExecuteCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
	{
		var currentUser = await _context.Users.FindAsync(new object[] { message.From!.Id }, cancellationToken);

		var spaceIndex = message.Text!.IndexOf(' ', StringComparison.Ordinal);
		if (spaceIndex < 0)
			return null;

		var dateText = message.Text[(spaceIndex + 1)..];

		if (!DateTime.TryParseExact(dateText, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthdayDate))
		{
			if (!DateTime.TryParseExact(dateText + "-0004", "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthdayDate))
				return Lines.BirthdaySetFailed;
		} else
		{
			if ((birthdayDate.AddYears(1) > DateTime.UtcNow.Date) || (birthdayDate.Year < 1900))
				return Lines.BirthdaySetFailed;
		}

		currentUser!.BirthdayDay = (byte) birthdayDate.Day;
		currentUser.BirthdayMonth = (byte) birthdayDate.Month;
		currentUser.BirthdayYear = birthdayDate.Year == 0004 ? null : (ushort?) birthdayDate.Year;

		await _context.SaveChangesAsync(cancellationToken);

		return Lines.BirthdaySetSuccessfully;
	}
}
