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
	private readonly DataContext _dataContext;
	private readonly ILogger<PostBirthdaysJob> _logger;

	public PostBirthdaysJob(ITelegramBotClient botClient, DataContext dataContext, ILogger<PostBirthdaysJob> logger)
	{
		_botClient = botClient;
		_dataContext = dataContext;
		_logger = logger;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		var scheduleDate = context.ScheduledFireTimeUtc!.Value.UtcDateTime;
		var scheduleHours = scheduleDate.Hour;

		foreach (var chat in _dataContext.Chats.Where(
			         chat => (chat.TimeZoneHourOffset + chat.CustomOffsetInHours + 24) % 24 == scheduleHours))
		{
			var totalOffsetHours = chat.TimeZoneHourOffset + chat.CustomOffsetInHours;

			int daysOffset;
			if (totalOffsetHours < 0)
			{
				daysOffset = 1;
			} else
			{
				daysOffset = -(totalOffsetHours / 24);
			}

			var birthdayDay = scheduleDate.Date.AddDays(daysOffset);
			var users = _dataContext.Users
				.Where(
					user => (user.BirthdayDay == birthdayDay.Day) && (user.BirthdayMonth == birthdayDay.Month) &&
						user.Chats.Contains(chat))
				.ToList();

			if (users.Count == 0)
				continue;

			var usernamesToPost = users.Select(
				x => $"<a href=\"tg://user?id={x.Id}\">" + HttpUtility.HtmlEncode(x.FirstName) + "</a>");

			try
			{
				var message = await _botClient.SendTextMessageAsync(
					chat.Id, string.Format(CultureInfo.CurrentCulture, Lines.BirthdayMessage, string.Join(", ", usernamesToPost)),
					parseMode: ParseMode.Html
				);

				await _botClient.PinChatMessageAsync(chat.Id, message.MessageId, true);

				await _dataContext.SentMessages.AddAsync(
					new SentMessage {ChatId = chat.Id, MessageId = message.MessageId, SendDateUtc = scheduleDate});
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
}
