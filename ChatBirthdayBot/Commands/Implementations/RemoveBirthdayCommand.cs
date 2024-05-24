using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using ChatBirthdayBot.Localization;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot.Commands;

public class RemoveBirthdayCommand : ICommand
{
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public RemoveBirthdayCommand(IServiceScopeFactory serviceScopeFactory) => _serviceScopeFactory = serviceScopeFactory;

	public string CommandName => "removebirthday";

	public async Task<string?> ExecuteCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
	{
		await using var scope = _serviceScopeFactory.CreateAsyncScope();
		var context = scope.ServiceProvider.GetRequiredService<DataContext>();

		var currentUser = await context.Users.FindAsync([message.From!.Id], cancellationToken);

		if (currentUser!.BirthdayDay == null)
		{
			return Lines.BirthdayNotSet;
		}

		currentUser.BirthdayYear = null;
		currentUser.BirthdayMonth = null;
		currentUser.BirthdayDay = null;

		await context.SaveChangesAsync(cancellationToken);

		return Lines.BirthdayRemoved;
	}

	public IReadOnlySet<ChatType> AllowedChatTypes { get; } = new HashSet<ChatType> {ChatType.Private};
}
