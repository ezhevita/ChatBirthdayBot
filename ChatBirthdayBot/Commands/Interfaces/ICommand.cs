using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot.Commands;

public interface ICommand
{
	public string CommandName { get; }

	public Task<string?> ExecuteCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);

	public Task HandleSentMessage(Message sentMessage) => Task.CompletedTask;

	public IReadOnlySet<ChatType> AllowedChatTypes { get; }
}
