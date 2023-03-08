using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ChatBirthdayBot.Database;
using ChatBirthdayBot.Localization;
using Microsoft.Extensions.Logging;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot.Tasks;

[DisallowConcurrentExecution]
public partial class PostBirthdaysJob : IJob
{
	private readonly ITelegramBotClient _botClient;
	private readonly DataContext _context;
	private readonly ILogger<PostBirthdaysJob> _logger;

	public PostBirthdaysJob(ITelegramBotClient botClient, DataContext context, ILogger<PostBirthdaysJob> logger)
	{
		_botClient = botClient;
		_context = context;
		_logger = logger;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		var scheduleDate = context.ScheduledFireTimeUtc!.Value.UtcDateTime;
		var scheduleHours = scheduleDate.Hour;
		_logger.LogDebug("Starting job @ {Hours}", scheduleHours);

		foreach (var chat in _context.Chats)
		{
			var chatTimezoneOffsetHours = (int)chat.TimeZoneOffset.TotalHours;
			var chatCustomOffset = chat.CustomOffsetInHours;
			var totalOffsetHours = chatTimezoneOffsetHours + chatCustomOffset;

			if ((totalOffsetHours + 24) % 24 != scheduleHours)
			{
				_logger.LogDebug(
					"Ignored chat {ChatName} because {Offset} offset shouldn't fire at {Hours}", chat.Name, totalOffsetHours,
					scheduleHours
				);

				continue;
			}

			int daysOffset;
			if (totalOffsetHours < 0)
			{
				daysOffset = 1;
			} else
			{
				daysOffset = -(totalOffsetHours / 24);
			}

			var birthdayDay = scheduleDate.Date.AddDays(daysOffset);
			var users = _context.Users
				.Where(user => (user.BirthdayDay == birthdayDay.Day) && (user.BirthdayMonth == birthdayDay.Month) && user.Chats.Contains(chat))
				.ToList();

			LogExecuteJobForChat(chat.Id, scheduleHours, users.Count);

			if (!users.Any())
				continue;

			var usernamesToPost = users.Select(x => $"<a href=\"tg://user?id={x.Id}\">{HttpUtility.HtmlEncode(x.FirstName)}</a>");

			try
			{
				var message = await _botClient.SendTextMessageAsync(
					chat.Id, string.Format(CultureInfo.CurrentCulture, Lines.BirthdayMessage, string.Join(", ", usernamesToPost)),
					ParseMode.Html
				);

				await _botClient.PinChatMessageAsync(chat.Id, message.MessageId, true);
			}
			catch (Exception e)
			{
				LogPostError(e);
			}
		}
	}

	[LoggerMessage(
		EventId = (int)LogEventId.PostErrorOccurred, Level = LogLevel.Error,
		Message = "An error occurred while posting a birthday message"
	)]
	private partial void LogPostError(Exception ex);

	[LoggerMessage(
		EventId = (int)LogEventId.ExecutingPostBirthdaysJobForChat, Level = LogLevel.Information,
		Message = "Executing PostBirthdays job for chat {ChatId} at {Hour}h, found {Members} birthday members"
	)]
	private partial void LogExecuteJobForChat(long chatId, int hour, int members);
}
