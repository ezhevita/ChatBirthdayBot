using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ChatBirthdayBot.Database;
using ChatBirthdayBot.Localization;
using Microsoft.EntityFrameworkCore;
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

		foreach (var chat in _context.Chats.Include(x => x.Users))
		{
			var chatTimezoneOffsetHours = (int)chat.TimeZoneOffset.TotalHours;
			var chatCustomOffset = chat.CustomOffsetInHours;
			var totalOffsetHours = chatTimezoneOffsetHours + chatCustomOffset;

			if ((totalOffsetHours + 24) % 24 != scheduleHours)
				continue;

			int daysOffset;
			if (totalOffsetHours < 0)
			{
				daysOffset = 1;
			} else
			{
				daysOffset = -(totalOffsetHours / 24);
			}

			var birthdayDay = scheduleDate.Date.AddDays(daysOffset);
			var users = chat.Users
				.Where(user => (user.BirthdayDay == birthdayDay.Day) && (user.BirthdayMonth == birthdayDay.Month))
				.ToList();

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

	[LoggerMessage(Level = LogLevel.Error, Message = "An error occurred while posting a birthday message")]
	private partial void LogPostError(Exception ex);
}