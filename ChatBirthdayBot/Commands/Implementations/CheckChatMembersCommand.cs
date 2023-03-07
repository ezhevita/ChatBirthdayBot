using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot.Commands;

public class CheckChatMembersCommand : ICommand
{
	private readonly DataContext _context;
	private readonly long _ownerId;

	public CheckChatMembersCommand(DataContext context, IOptions<BotConfiguration> botConfig)
	{
		_context = context;
		_ownerId = botConfig.Value.UserOwnerId;
	}

	public string CommandName => "checkmembers";
	public bool ShouldBeExecutedForChatType(ChatType chatType) => chatType is ChatType.Supergroup;

	public async Task<string?> ExecuteCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
	{
		var chatId = message.Chat.Id;
		var currentChatMember = await botClient.GetChatMemberAsync(chatId, message.From!.Id, cancellationToken);

		if (currentChatMember.Status is not ChatMemberStatus.Administrator or ChatMemberStatus.Creator &&
		    message.From.Id != _ownerId)
			return null;

		var chat = await _context.Chats
			.Include(x => x.UserChats)
			.FirstOrDefaultAsync(chat => chat.Id == chatId, cancellationToken: cancellationToken);

		if (chat == null)
			return null;

		var membersToRemove = new List<UserChat>();
		foreach (var member in chat.UserChats)
		{
			try
			{
				var chatMember = await botClient.GetChatMemberAsync(chatId, member.UserId, cancellationToken);
				if (chatMember.Status is ChatMemberStatus.Kicked or ChatMemberStatus.Left)
				{
					membersToRemove.Add(member);
				}
			}
			catch (ApiRequestException e) when (e.Message.Contains("user not found", StringComparison.Ordinal))
			{
				membersToRemove.Add(member);
			}
		}

		_context.UserChats.RemoveRange(membersToRemove);

		await _context.SaveChangesAsync(cancellationToken);

		return $"{membersToRemove.Count} chat members removed";
	}
}
