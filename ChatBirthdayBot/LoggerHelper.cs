using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot;

public static partial class LoggerHelper
{
	[LoggerMessage(
		EventId = (int)LogEventId.ChatMemberUpdateReceived, Level = LogLevel.Debug,
		Message = "New status of the user {UserId} in chat {ChatId} - {Status}")]
	private static partial void LogChatMemberUpdate(ILogger logger, long userId, long chatId, ChatMemberStatus status);

	private static void LogChatMemberUpdate(ILogger logger, ChatMemberUpdated chatMemberUpdated)
	{
		LogChatMemberUpdate(logger, chatMemberUpdated.Chat.Id, chatMemberUpdated.From.Id, chatMemberUpdated.NewChatMember.Status);
	}

	public static void LogCommandMessage(this ILogger logger, Message message)
	{
		if (message.Chat.Type == ChatType.Private)
		{
			LogCommandMessageFromPrivate(logger, message.From!.Id, message.Text!);
		} else
		{
			LogCommandMessageFromChat(logger, message.Chat.Id, message.From!.Id, message.Text!);
		}
	}

	[LoggerMessage(
		EventId = (int)LogEventId.ChatCommandMessageReceived, Level = LogLevel.Debug,
		Message = "Chat message from {ChatId} by {UserId} with a text '{Text}'")]
	private static partial void LogCommandMessageFromChat(ILogger logger, long chatId, long userId, string text);

	[LoggerMessage(
		EventId = (int)LogEventId.PrivateCommandMessageReceived, Level = LogLevel.Debug,
		Message = "Private message from {UserId} with a text '{Text}'")]
	private static partial void LogCommandMessageFromPrivate(ILogger logger, long userId, string text);

	[LoggerMessage(
		EventId = (int)LogEventId.MyChatMemberUpdateReceived, Level = LogLevel.Debug,
		Message = "New status of the bot in chat {ChatId} - {Status}")]
	private static partial void LogMyChatMemberUpdate(ILogger logger, long chatId, ChatMemberStatus status);

	private static void LogMyChatMemberUpdate(ILogger logger, ChatMemberUpdated chatMemberUpdated)
	{
		LogMyChatMemberUpdate(logger, chatMemberUpdated.Chat.Id, chatMemberUpdated.NewChatMember.Status);
	}

	public static void LogNonMessageUpdate(this ILogger logger, Update update)
	{
		switch (update.Type)
		{
			case UpdateType.MyChatMember:
				LogMyChatMemberUpdate(logger, update.MyChatMember!);

				break;
			case UpdateType.ChatMember:
				LogChatMemberUpdate(logger, update.ChatMember!);

				break;
		}
	}
}
