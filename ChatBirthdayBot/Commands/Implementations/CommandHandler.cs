using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot.Commands;

public sealed partial class CommandHandler : ICommandHandler
{
	private readonly BotUserData _botUserData;
	private readonly Dictionary<string, ICommand> _commands;
	private readonly ILogger<CommandHandler> _logger;

	public CommandHandler(IEnumerable<ICommand> commands, ILogger<CommandHandler> logger, BotUserData botUserData)
	{
		_logger = logger;
		_commands = commands.ToDictionary(x => x.CommandName.ToUpperInvariant(), x => x);
		_botUserData = botUserData;
	}

	public async Task Execute(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
	{
		var messageText = message.Text;

		if (string.IsNullOrEmpty(messageText))
			return;

		var args = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		var commandName = args[0][1..];

		if (string.IsNullOrEmpty(commandName))
			return;

		var indexOfAt = commandName.IndexOf('@', StringComparison.Ordinal);
		if (indexOfAt >= 0)
		{
			var botName = commandName[(indexOfAt + 1)..];
			if (!string.IsNullOrEmpty(botName))
			{
				if (!botName.Equals(_botUserData.Username, StringComparison.OrdinalIgnoreCase))
					return;
			}

			commandName = commandName[..indexOfAt];
		}

		if (!_commands.TryGetValue(commandName.ToUpperInvariant(), out var command))
			return;

		if (!command.AllowedChatTypes.Contains(message.Chat.Type))
			return;

		var response = await command.ExecuteCommand(botClient, message, cancellationToken);

		if (string.IsNullOrEmpty(response))
			return;

		try
		{
			var sentMessage = await botClient.SendTextMessageAsync(
				message.Chat.Id, response, parseMode: ParseMode.Html, replyToMessageId: message.MessageId,
				cancellationToken: cancellationToken);

			await command.HandleSentMessage(sentMessage);
		}
		catch (Exception e)
		{
			LogRespondingError(e);
		}
	}

	[LoggerMessage(
		EventId = (int)LogEventId.RespondErrorOccurred, Level = LogLevel.Error,
		Message = "Failed sending the response with an exception")]
	private partial void LogRespondingError(Exception ex);
}
