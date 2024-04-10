using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot;

public class ReceiverService
{
	private readonly ITelegramBotClient _botClient;
	private readonly BotUserData _botUserData;
	private readonly IUpdateHandler _updateHandlers;

	public ReceiverService(
		ITelegramBotClient botClient,
		IUpdateHandler updateHandler,
		BotUserData botUserData)
	{
		_botClient = botClient;
		_updateHandlers = updateHandler;
		_botUserData = botUserData;
	}

	public async Task ReceiveAsync(CancellationToken stoppingToken)
	{
		var receiverOptions = new ReceiverOptions
		{
			AllowedUpdates = [UpdateType.Message, UpdateType.ChatMember, UpdateType.MyChatMember],
			ThrowPendingUpdates = false
		};

		var me = await _botClient.GetMeAsync(stoppingToken);

		_botUserData.Username = me.Username!;

		await _botClient.ReceiveAsync(_updateHandlers, receiverOptions, stoppingToken);
	}
}
