using System;
using System.Linq;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace ChatBirthdayBot.Tasks;

public class UnpinBirthdaysMessagesJob : IJob
{
	private readonly ITelegramBotClient _botClient;
	private readonly DataContext _dataContext;
	private readonly ILogger<UnpinBirthdaysMessagesJob> _logger;

	public UnpinBirthdaysMessagesJob(ITelegramBotClient botClient, DataContext dataContext,
		ILogger<UnpinBirthdaysMessagesJob> logger)
	{
		_botClient = botClient;
		_dataContext = dataContext;
		_logger = logger;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		var date = DateTime.UtcNow - TimeSpan.FromDays(1);
		var messagesToUnpin = await _dataContext.SentMessages.Where(e => e.SendDateUtc < date).ToListAsync();
		foreach (var messageToUnpin in messagesToUnpin)
		{
			try
			{
				await _botClient.UnpinChatMessageAsync(messageToUnpin.ChatId, messageToUnpin.MessageId);
			}
			catch (ApiRequestException)
			{
			}
		}

		_dataContext.SentMessages.RemoveRange(messagesToUnpin);
		await _dataContext.SaveChangesAsync();
	}
}
