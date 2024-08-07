using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot.Tasks;

public partial class CheckChatMembersJob : IJob
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ITelegramBotClient _telegramBotClient;
	private readonly ILogger<CheckChatMembersJob> _logger;

	public CheckChatMembersJob(IServiceScopeFactory serviceScopeFactory, ITelegramBotClient telegramBotClient,
		ILogger<CheckChatMembersJob> logger)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_telegramBotClient = telegramBotClient;
		_logger = logger;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		await using var scope = _serviceScopeFactory.CreateAsyncScope();
		var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

		var membersToRemove = new List<UserChat>();
		var batchMembersToRemove = new List<UserChat>();

		var botAccount = await _telegramBotClient.GetMeAsync();

		var batch = await dataContext.UserChats
			.OrderBy(x => x.ChatId)
			.ThenBy(x => x.UserId)
			.Take(100)
			.ToListAsync();

		var cachedBotStatus = new Dictionary<long, ChatMemberStatus>();
		var i = 0;
		while (batch.Count > 0)
		{
			foreach (var userChat in batch)
			{
				if (context.CancellationToken.IsCancellationRequested)
					return;

				if (!cachedBotStatus.TryGetValue(userChat.ChatId, out var status))
				{
					try
					{
						status = (await _telegramBotClient.GetChatMemberAsync(userChat.ChatId, botAccount.Id)).Status;
					}
					catch (ApiRequestException)
					{
						// Probably chat no longer exists for bot
						status = ChatMemberStatus.Kicked;
					}

					cachedBotStatus[userChat.ChatId] = status;
				}

				if (status != ChatMemberStatus.Administrator)
				{
					// Some unexpected things might happen, telegram doesn't say exactly which ones :shrug:
					continue;
				}

				try
				{
					var chatMember = await _telegramBotClient.GetChatMemberAsync(userChat.ChatId, userChat.UserId);
					if (chatMember.Status is ChatMemberStatus.Kicked or ChatMemberStatus.Left)
					{
						batchMembersToRemove.Add(userChat);
					}
				}
				catch (ApiRequestException e) when (e.Message.Contains("user not found", StringComparison.Ordinal))
				{
					batchMembersToRemove.Add(userChat);
				}
				catch (Exception e)
				{
					LogChatMemberCheckFailed(e, userChat.UserId, userChat.ChatId);
				}
			}

			membersToRemove.AddRange(batchMembersToRemove);
			batchMembersToRemove.Clear();

			i++;
			batch = await dataContext.UserChats
				.OrderBy(x => x.ChatId)
				.ThenBy(x => x.UserId)
				.Skip(i * 100)
				.Take(100)
				.ToListAsync();
		}

		dataContext.UserChats.RemoveRange(membersToRemove);
		await dataContext.SaveChangesConcurrentAsync(context.CancellationToken);

		LogChatMembersRemoved(membersToRemove.Count);
	}

	[LoggerMessage(LogLevel.Information, "{MembersRemoved} chat members removed", EventId = (int)LogEventId.ChatMembersRemoved)]
	private partial void LogChatMembersRemoved(int membersRemoved);

	[LoggerMessage(LogLevel.Warning, "Chat member check failed, user {UserID} for chat {ChatID}",
		EventId = (int)LogEventId.ChatMemberCheckFailed)]
	private partial void LogChatMemberCheckFailed(Exception exception, long userID, long chatID);
}
