using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ChatBirthdayBot.Database;
using ChatBirthdayBot.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot.Commands;

public class ListBirthdaysCommand : ICommand
{
	private static readonly ConcurrentDictionary<long, int> _lastSentBirthdaysMessage = new();
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public ListBirthdaysCommand(IServiceScopeFactory serviceScopeFactory) => _serviceScopeFactory = serviceScopeFactory;

	public string CommandName => "birthdays";

	public async Task<string?> ExecuteCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
	{
		if (_lastSentBirthdaysMessage.TryGetValue(message.Chat.Id, out var messageID))
		{
			botClient.DeleteMessageAsync(message.Chat.Id, messageID, cancellationToken).RunInBackgroundSuppressingExceptions();
		}

		var birthdays = await GetNearestBirthdaysForChat(message.Chat.Id, cancellationToken);

		if (birthdays.Count == 0)
			return Lines.NoBirthdays;

		return string.Join(
			'\n', birthdays.Select(
				x =>
				{
					DateTime birthdayDate = new(x.BirthdayYear ?? 0004, x.BirthdayMonth!.Value, x.BirthdayDay!.Value);

					return $"<b>{birthdayDate.ToString("d MMM", CultureInfo.CurrentCulture)}</b> — {Escape(x.FirstName)}" +
						$"{(x.LastName != null ? " " + Escape(x.LastName) : "")}" +
						$"{(x.BirthdayYear != null ? $" <b>({AgeFromDate(birthdayDate) + 1})</b>" : "")}";
				}));
	}

	public Task HandleSentMessage(Message sentMessage)
	{
		_lastSentBirthdaysMessage[sentMessage.Chat.Id] = sentMessage.MessageId;

		return Task.CompletedTask;
	}

	public IReadOnlySet<ChatType> AllowedChatTypes { get; } = new HashSet<ChatType> {ChatType.Group, ChatType.Supergroup};

	private static int AgeFromDate(DateTime birthdate)
	{
		var today = DateTime.UtcNow.Date;
		var age = (byte)(today.Year - birthdate.Year);
		if (birthdate.Date > today.AddYears(-age))
		{
			age--;
		}

		if ((birthdate.Day == today.Day) && (birthdate.Month == today.Month))
		{
			age--;
		}

		return age;
	}

	private static string Escape(string message) => HttpUtility.HtmlEncode(message);

	private async Task<List<UserRecord>> GetNearestBirthdaysForChat(long chatID, CancellationToken cancellationToken)
	{
		var date = DateTime.UtcNow.AddHours(3);
		var key = (ushort)((date.Month << 5) + date.Day);

		await using var scope = _serviceScopeFactory.CreateAsyncScope();
		var context = scope.ServiceProvider.GetRequiredService<DataContext>();

		return await context.UserChats
			.Include(x => x.User)
			.Where(x => (x.ChatId == chatID) && (x.User.BirthdayDay != null) && (x.User.BirthdayMonth != null))
			.Select(userChat => new {userChat, tempKey = userChat.User.BirthdayMonth * 32 + userChat.User.BirthdayDay})
			.OrderBy(x => x.tempKey < key ? x.tempKey + 12 * 32 : x.tempKey)
			.Select(x => x.userChat.User)
			.Take(10)
			.ToListAsync(cancellationToken: cancellationToken);
	}
}
