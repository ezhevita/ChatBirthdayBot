using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using ChatBirthdayBot.Localization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot.Commands;

public class RemoveBirthdayCommand : ICommand
{
	private readonly DataContext _context;

	public RemoveBirthdayCommand(DataContext context)
	{
		_context = context;
	}

	public string CommandName => "removebirthday";

	public bool ShouldBeExecutedForChatType(ChatType chatType) => chatType == ChatType.Private;

	public async Task<string?> ExecuteCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
	{
		var currentUser = await _context.Users.FindAsync(new object[] {message.From!.Id}, cancellationToken);

		if (currentUser!.BirthdayDay != null)
		{
			currentUser.BirthdayYear = null;
			currentUser.BirthdayMonth = null;
			currentUser.BirthdayDay = null;

			await _context.SaveChangesAsync(cancellationToken);

			return Lines.BirthdayRemoved;
		}

		return Lines.BirthdayNotSet;
	}
}
