using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot.Commands;

public interface ICommand
{
	public string CommandName { get; }

	public bool ShouldBeExecutedForChatType(ChatType chatType);

	public Task<string?> ExecuteCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);

	public Task HandleSentMessage(Message sentMessage) => Task.CompletedTask;
}
