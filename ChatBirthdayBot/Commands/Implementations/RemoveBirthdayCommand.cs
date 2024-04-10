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

	public RemoveBirthdayCommand(IServiceScopeFactory serviceScopeFactory)
	{
		_serviceScopeFactory = serviceScopeFactory;
	}

	public string CommandName => "removebirthday";

	public bool ShouldBeExecutedForChatType(ChatType chatType) => chatType == ChatType.Private;

	public async Task<string?> ExecuteCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
	{
		await using var scope = _serviceScopeFactory.CreateAsyncScope();
		var context = scope.ServiceProvider.GetRequiredService<DataContext>();

		var currentUser = await context.Users.FindAsync(new object[] {message.From!.Id}, cancellationToken);

		if (currentUser!.BirthdayDay != null)
		{
			currentUser.BirthdayYear = null;
			currentUser.BirthdayMonth = null;
			currentUser.BirthdayDay = null;

			await context.SaveChangesAsync(cancellationToken);

			return Lines.BirthdayRemoved;
		}

		return Lines.BirthdayNotSet;
	}
}
