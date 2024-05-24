using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using ChatBirthdayBot.Localization;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot.Commands;

public class BirthdayInfoCommand : ICommand
{
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public BirthdayInfoCommand(IServiceScopeFactory serviceScopeFactory) => _serviceScopeFactory = serviceScopeFactory;

	public virtual string CommandName => "birthday";
	public IReadOnlySet<ChatType> AllowedChatTypes { get; } = new HashSet<ChatType> {ChatType.Private};

	public async Task<string?> ExecuteCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
	{
		UserRecord? currentUser;
		await using (var scope = _serviceScopeFactory.CreateAsyncScope())
		{
			var context = scope.ServiceProvider.GetRequiredService<DataContext>();
			currentUser = await context.Users.FindAsync([message.From!.Id], cancellationToken);
		}

		if (currentUser is not {BirthdayDay: not null, BirthdayMonth: not null})
			return Lines.BirthdayNotSet;

		DateTime date = new(currentUser.BirthdayYear ?? 0004, currentUser.BirthdayMonth.Value, currentUser.BirthdayDay.Value);

		return string.Format(
			CultureInfo.CurrentCulture, Lines.BirthdayDate,
			date.Year == 0004 ? date.ToString("M", CultureInfo.CurrentCulture) : date.ToLongDateStringWithoutDayOfWeek());
	}
}
