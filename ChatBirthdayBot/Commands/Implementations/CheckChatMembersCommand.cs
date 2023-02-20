using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot.Commands;

public class CheckChatMembersCommand : ICommand
{
	private readonly DataContext _context;

	public CheckChatMembersCommand(DataContext context)
	{
		_context = context;
	}

	public string CommandName => "checkmembers";
	public bool ShouldBeExecutedForChatType(ChatType chatType) => chatType is ChatType.Supergroup;

	public async Task<string?> ExecuteCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
	{
		var chatId = message.Chat.Id;

		var chat = await _context.Chats.FindAsync(new object[] { chatId }, cancellationToken: cancellationToken);

		if (chat == null)
			return null;

		var membersToRemove = new List<UserChat>();
		foreach (var member in chat.UserChats)
		{
			var chatMember = await botClient.GetChatMemberAsync(chatId, member.UserId, cancellationToken);
			if (chatMember.Status is ChatMemberStatus.Kicked or ChatMemberStatus.Left)
			{
				membersToRemove.Add(member);
			}
		}

		_context.UserChats.RemoveRange(membersToRemove);

		await _context.SaveChangesAsync(cancellationToken);

		return "✅";
	}
}
