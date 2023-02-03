using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChatBirthdayBot.Commands;

public interface ICommandHandler
{
	Task Execute(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);
}
