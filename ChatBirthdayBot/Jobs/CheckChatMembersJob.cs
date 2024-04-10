using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
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
		await foreach (var userChat in dataContext.UserChats)
		{
			try
			{
				var chatMember = await _telegramBotClient.GetChatMemberAsync(userChat.ChatId, userChat.UserId);
				if (chatMember.Status is ChatMemberStatus.Kicked or ChatMemberStatus.Left)
				{
					membersToRemove.Add(userChat);
				}
			}
			catch (ApiRequestException e) when (e.Message.Contains("user not found", StringComparison.Ordinal))
			{
				membersToRemove.Add(userChat);
			}
			catch (Exception e)
			{
				LogChatMemberCheckFailed(e);
			}
		}

		dataContext.UserChats.RemoveRange(membersToRemove);
		await dataContext.SaveChangesAsync();

		LogChatMembersRemoved(membersToRemove.Count);
	}

	[LoggerMessage(LogLevel.Information, "{Count} chat members removed", EventId = (int)LogEventId.ChatMembersRemoved)]
	private partial void LogChatMembersRemoved(int membersRemoved);

	[LoggerMessage(LogLevel.Warning, "Chat member check failed", EventId = (int)LogEventId.ChatMemberCheckFailed)]
	private partial void LogChatMemberCheckFailed(Exception exception);
}
