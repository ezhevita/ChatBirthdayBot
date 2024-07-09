using System;
using System.Linq;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace ChatBirthdayBot.Tasks;

public class UnpinBirthdaysMessagesJob : IJob
{
	private readonly ITelegramBotClient _botClient;
	private readonly DataContext _dataContext;

	public UnpinBirthdaysMessagesJob(ITelegramBotClient botClient, DataContext dataContext)
	{
		_botClient = botClient;
		_dataContext = dataContext;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		var date = DateTime.UtcNow - TimeSpan.FromDays(1);
		var messagesToUnpin = await _dataContext.SentMessages.Where(e => e.SendDateUtc < date).ToListAsync();
		foreach (var messageToUnpin in messagesToUnpin)
		{
			if (context.CancellationToken.IsCancellationRequested)
				return;

			try
			{
				await _botClient.UnpinChatMessageAsync(messageToUnpin.ChatId, messageToUnpin.MessageId);
			}
			catch (ApiRequestException)
			{
			}
		}

		_dataContext.SentMessages.RemoveRange(messagesToUnpin);
		await _dataContext.SaveChangesConcurrentAsync(context.CancellationToken);
	}
}
