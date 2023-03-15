using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Commands;
using ChatBirthdayBot.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot;

public partial class UpdateHandler : IUpdateHandler
{
	private readonly ILogger<UpdateHandler> _logger;
	private readonly ICommandHandler _handler;
	private readonly DataContext _context;
	private readonly CultureInfo _russianCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("ru-RU");
	private readonly ConcurrentDictionary<long, DateTime> _dateTimesOfLastSentCommandInChat = new();

	public UpdateHandler(ILogger<UpdateHandler> logger, ICommandHandler handler, DataContext context)
	{
		_logger = logger;
		_handler = handler;
		_context = context;
	}

	public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
	{
		_logger.LogNonMessageUpdate(update);
		await ProcessDatabaseUpdates(update, cancellationToken);

		if ((update.Type != UpdateType.Message) || (update.Message!.Type != MessageType.Text))
		{
			return;
		}

		var message = update.Message;

		if (message is not {Type: MessageType.Text})
			return;

		var messageText = message.Text;

		if (string.IsNullOrEmpty(messageText))
			return;

		if (!messageText.StartsWith("/", StringComparison.Ordinal))
			return;

		if (message.From == null)
			return;

		_logger.LogCommandMessage(message);

		Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = message.From.LanguageCode switch
		{
			null or "" or "ru" or "uk" or "be" => _russianCulture,
			_ => CultureInfo.GetCultureInfoByIetfLanguageTag(message.From.LanguageCode)
		};

		if (_dateTimesOfLastSentCommandInChat.TryGetValue(message.Chat.Id, out var lastSentMessage) &&
		    (lastSentMessage.AddSeconds(3) > message.Date))
		{
			return;
		}

		_dateTimesOfLastSentCommandInChat[message.Chat.Id] = message.Date;

		await _handler.Execute(botClient, message, cancellationToken);

		await _context.SaveChangesAsync(cancellationToken);
	}

	public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
	{
		LogPollingError(exception);

		return Task.CompletedTask;
	}

	private async Task ProcessDatabaseUpdates(Update update, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(update);

		switch (update.Type)
		{
			case UpdateType.MyChatMember
				when update.MyChatMember!.NewChatMember.Status is ChatMemberStatus.Member or ChatMemberStatus.Administrator:
			{
				await UpdateChat(update.MyChatMember.Chat);

				break;
			}
			case UpdateType.MyChatMember
				when update.MyChatMember.NewChatMember.Status is ChatMemberStatus.Kicked or ChatMemberStatus.Left:
			{
				var chat = update.MyChatMember.Chat;
				var dbChat = await _context.Chats
					.Include(x => x.UserChats)
					.FirstOrDefaultAsync(x => x.Id == chat.Id, cancellationToken);

				if (dbChat != null)
				{
					dbChat.UserChats.Clear();
					_context.Chats.Remove(dbChat);
				}

				break;
			}
			case UpdateType.ChatMember
				when update.ChatMember!.NewChatMember.Status is ChatMemberStatus.Member or ChatMemberStatus.Administrator:
			{
				var user = update.ChatMember.NewChatMember.User;
				var chat = update.ChatMember.Chat;
				await UpdateUser(user);
				await UpdateChat(chat);
				await UpdateUserChat(chat, user);

				break;
			}
			case UpdateType.ChatMember
				when update.ChatMember.NewChatMember.Status is ChatMemberStatus.Kicked or ChatMemberStatus.Left:
			{
				var user = update.ChatMember.NewChatMember.User;
				var chat = update.ChatMember.Chat;
				var participant = await _context.UserChats.FindAsync(new object[] {user.Id, chat.Id}, cancellationToken);
				if (participant != null)
				{
					_context.UserChats.Remove(participant);
				}

				await UpdateUser(user);
				await UpdateChat(chat);

				break;
			}
			case UpdateType.Message when update.Message!.Chat.Type is ChatType.Private:
			{
				await UpdateUser(update.Message.From!);

				break;
			}
			case UpdateType.Message when update.Message.Chat.Type is ChatType.Supergroup or ChatType.Group:
			{
				await UpdateUser(update.Message.From!);
				await UpdateChat(update.Message.Chat);
				await UpdateUserChat(update.Message.Chat, update.Message.From!);

				break;
			}
		}

		await _context.SaveChangesAsync(cancellationToken);
	}

	private async Task UpdateUser(User user)
	{
		await _context.Upsert(
			new UserRecord
			{
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Username = user.Username
			}
		).WhenMatched(
			x => new UserRecord
			{
				BirthdayDay = x.BirthdayDay,
				BirthdayMonth = x.BirthdayMonth,
				BirthdayYear = x.BirthdayYear,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Username = user.Username
			}
		).RunAsync();
	}

	private async Task UpdateUserChat(Chat chat, User user)
	{
		var participantExists = await _context.UserChats.AnyAsync(x => (x.ChatId == chat.Id) && (x.UserId == user.Id));
		if (!participantExists)
		{
			UserChat participant = new()
			{
				ChatId = chat.Id,
				UserId = user.Id,
				IsPublic = true
			};

			_context.UserChats.Add(participant);
		}
	}

	private Task UpdateChat(Chat chat) =>
		_context.Upsert(
			new ChatRecord
			{
				Id = chat.Id,
				Name = chat.Title
			}
		).WhenMatched(
			x => new ChatRecord
			{
				CustomOffsetInHours = x.CustomOffsetInHours,
				Locale = x.Locale,
				Name = chat.Title,
				ShouldPinNotify = x.ShouldPinNotify,
				TimeZoneHourOffset = x.TimeZoneHourOffset
			}
		).RunAsync();

	[LoggerMessage(
		EventId = (int)LogEventId.PollingErrorOccurred, Level = LogLevel.Error,
		Message = "Polling error occured with an exception"
	)]
	private partial void LogPollingError(Exception ex);
}
