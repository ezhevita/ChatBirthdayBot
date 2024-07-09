using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Commands;
using ChatBirthdayBot.Localization;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot;

public class ReceiverService
{
	private readonly ITelegramBotClient _botClient;
	private readonly BotUserData _botUserData;
	private readonly IEnumerable<ICommand> _commands;
	private readonly IUpdateHandler _updateHandlers;

	public ReceiverService(
		ITelegramBotClient botClient,
		IUpdateHandler updateHandler,
		BotUserData botUserData,
		IEnumerable<ICommand> commands)
	{
		_botClient = botClient;
		_updateHandlers = updateHandler;
		_botUserData = botUserData;
		_commands = commands;
	}

	public async Task ReceiveAsync(CancellationToken stoppingToken)
	{
		var receiverOptions = new ReceiverOptions
		{
			AllowedUpdates = [UpdateType.Message, UpdateType.ChatMember, UpdateType.MyChatMember],
			ThrowPendingUpdates = false
		};

		var me = await _botClient.GetMeAsync(stoppingToken);

		foreach (var chatType in new[] {ChatType.Private, ChatType.Supergroup})
		{
			var commands = _commands.Where(command => command.AllowedChatTypes.Contains(chatType)).ToList();

			foreach (var locale in new[] {null, "ru-RU"})
			{
				await _botClient.SetMyCommandsAsync(
					BotCommandsForLocale(commands, locale), Utilities.ChatTypeToCommandScope(chatType), locale, stoppingToken);
			}
		}

		_botUserData.Username = me.Username!;

		await _botClient.ReceiveAsync(_updateHandlers, receiverOptions, stoppingToken);
	}

	private static IEnumerable<BotCommand> BotCommandsForLocale(IEnumerable<ICommand> commands, string? locale)
	{
		var resourceManager = Lines.ResourceManager;
		return commands.Select(
			command => new BotCommand
			{
				Command = command.CommandName,
				Description = resourceManager.GetString(
					command.CommandDescriptionLocalizationKey, locale == null ? null : new CultureInfo(locale))!
			});
	}
}
