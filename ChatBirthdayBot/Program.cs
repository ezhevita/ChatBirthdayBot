using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using ChatBirthdayBot.Localization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Chat = ChatBirthdayBot.Database.Chat;
using File = System.IO.File;
using User = ChatBirthdayBot.Database.User;

namespace ChatBirthdayBot {
	internal static class Program {
		private static TelegramBotClient Bot = null!;
		private static readonly SemaphoreSlim ShutdownSemaphore = new(0, 1);
		private static readonly CultureInfo RussianCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("ru-RU");

		private static int AgeFromDate(DateTime birthdate) {
			var today = DateTime.Today;
			var age = today.Year - birthdate.Year;
			if (birthdate.Date > today.AddYears(-age)) {
				age--;
			}

			return age;
		}

		private static async Task CheckBirthdays() {
			DateTime currentDate = DateTime.UtcNow.Date;

			byte month = (byte) currentDate.Month;
			byte day = (byte) currentDate.Day;

			DataContext context = new();
			List<User> todayBirthdays;
			await using (context.ConfigureAwait(false)) {
				todayBirthdays = await context.Users
					.Where(user => (user.BirthdayDay == day) && (user.BirthdayMonth == month) && (user.Chats.Count > 0))
					.Include(x => x.Chats)
					.ToListAsync()
					.ConfigureAwait(false);
			}

			Dictionary<Chat, List<User>> dictionary = todayBirthdays
				.Select(user => user.Chats.Select(chat => new { chat, user }))
				.SelectMany(x => x)
				.GroupBy(x => x.chat)
				.ToDictionary(x => x.Key, x => x.Select(y => y.user).ToList());

			foreach ((Chat chat, List<User> users) in dictionary) {
				IEnumerable<string> usernamesToPost = users.Select(x => $"[{x.FirstName}](tg://user?id={x.Id})");
				await Bot.SendTextMessageAsync(chat.Id, string.Join(", ", usernamesToPost) + " - с днём рождения!", ParseMode.Markdown).ConfigureAwait(false);
			}
		}

		private static Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
			Console.WriteLine(exception);
			return Task.CompletedTask;
		}

		private static async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
			Console.WriteLine($@"Update|{JsonConvert.SerializeObject(update)}");
			DataContext context = new();
			await using (context.ConfigureAwait(false)) {
				await ProcessDatabaseUpdates(context, update, cancellationToken).ConfigureAwait(false);
				await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			}

			if ((update.Type != UpdateType.Message) || (update.Message.Type != MessageType.Text)) {
				return;
			}

			Message message = update.Message;
			string messageText = message.Text;
			if (string.IsNullOrEmpty(messageText) || (messageText[0] != '/')) {
				return;
			}

			Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = message.From.LanguageCode switch {
				null or "" or "ru" or "uk" or "be" => RussianCulture,
				_ => CultureInfo.GetCultureInfoByIetfLanguageTag(message.From.LanguageCode)
			};

			string text = "";
			ParseMode parseMode = ParseMode.Default;

			context = new DataContext();
			try {
				User? currentUser = await context.Users.FindAsync(message.From.Id).ConfigureAwait(false);
				if (currentUser == null) {
					return;
				}

				string[] args = messageText.ToUpperInvariant().Split(new[] { ' ', '@' }, StringSplitOptions.RemoveEmptyEntries);
				switch (args[0].ToUpperInvariant()) {
					case "/BIRTHDAYS" when message.Chat.Type is ChatType.Group or ChatType.Supergroup: {
						var date = DateTime.UtcNow.AddHours(3);
						var key = ((byte) date.Month << 5) + (byte) date.Day;

						var birthdays = await context.UserChats
							.Include(x => x.User)
							.Where(x => (x.ChatId == message.Chat.Id) && (x.User.BirthdayDay != null) && (x.User.BirthdayMonth != null))
							.Select(userChat => new {userChat, tempKey = userChat.User.BirthdayMonth * 32 + userChat.User.BirthdayDay})
							.OrderBy(x => x.tempKey < key ? x.tempKey + 384 : x.tempKey)
							.Select(x => x.userChat.User)
							.Take(10)
							.ToListAsync(cancellationToken: cancellationToken);

						text = string.Join(
							'\n',
							birthdays.Select(
								x => {
									DateTime birthdayDate = new(x.BirthdayYear ?? 0001, x.BirthdayMonth!.Value, x.BirthdayDay!.Value);

									return $"*{birthdayDate.ToString("d MMM", CultureInfo.CurrentCulture)}* — {x.FirstName}{(x.LastName != null ? " " + x.LastName : "")}{(x.BirthdayYear != null ? $" *({AgeFromDate(birthdayDate) + 1})*" : "")}";
								}
							)
						);

						parseMode = ParseMode.Markdown;
						break;
					}
					case "/BIRTHDAY" when message.Chat.Type is ChatType.Private: {
						if (currentUser is { BirthdayDay: not null, BirthdayMonth: not null }) {
							DateTime date = new(currentUser.BirthdayYear ?? 0001, currentUser.BirthdayMonth.Value, currentUser.BirthdayDay.Value);
							text = string.Format(
								CultureInfo.CurrentCulture, Lines.BirthdayDate, date.Year == 0001
									? date.ToString("M", CultureInfo.CurrentCulture)
									: date.ToLongDateString().Replace(CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(date.DayOfWeek), "").TrimStart(',', ' ').TrimEnd('.')
							);
						} else {
							text = Lines.BirthdayNotSet;
						}

						parseMode = ParseMode.Markdown;

						break;
					}

					case "/SETBIRTHDAY" when (args.Length > 1) && message.Chat.Type is ChatType.Private: {
						string dateText = args[1];
						DateTime birthdayDate;
						if (!DateTime.TryParseExact(dateText, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedBirthdayDate)) {
							if (!DateTime.TryParseExact(dateText, "dd-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedBirthdayDate)) {
								text = Lines.BirthdaySetFailed;

								break;
							}

							birthdayDate = new DateTime(0001, parsedBirthdayDate.Month, parsedBirthdayDate.Day);
						} else {
							birthdayDate = parsedBirthdayDate.Year < 1900 ? new DateTime(0001, parsedBirthdayDate.Month, parsedBirthdayDate.Day) : parsedBirthdayDate;
						}

						currentUser.BirthdayDay = (byte) birthdayDate.Day;
						currentUser.BirthdayMonth = (byte) birthdayDate.Month;
						currentUser.BirthdayYear = (ushort) birthdayDate.Year;

						text = Lines.BirthdaySetSuccessfully;

						break;
					}

					case "/REMOVEBIRTHDAY" when message.Chat.Type is ChatType.Private: {
						if (currentUser.BirthdayDay != null) {
							currentUser.BirthdayDay = null;
							currentUser.BirthdayMonth = null;
							currentUser.BirthdayYear = null;

							text = Lines.BirthdayRemoved;
						} else {
							text = Lines.BirthdayNotSet;
						}

						break;
					}
				}
			} finally {
				await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
				await context.DisposeAsync().ConfigureAwait(false);
				if (!string.IsNullOrEmpty(text)) {
					await Bot.SendTextMessageAsync(message.Chat.Id, text, parseMode, replyToMessageId: message.MessageId, cancellationToken: cancellationToken).ConfigureAwait(false);
				}
			}
		}

		private static async Task Main() {
			Bot = new TelegramBotClient(await File.ReadAllTextAsync("token.txt").ConfigureAwait(false));
			Timer birthdayTimer = new(
				async _ => await CheckBirthdays().ConfigureAwait(false),
				null,
				DateTime.Today.AddDays(1).AddHours(3) - DateTime.UtcNow,
				TimeSpan.FromDays(1)
			);

			Bot.StartReceiving(new DefaultUpdateHandler(HandleUpdate, HandleError, new[] { UpdateType.Message, UpdateType.ChatMember, UpdateType.MyChatMember }));

			await CheckBirthdays().ConfigureAwait(false);
			await ShutdownSemaphore.WaitAsync().ConfigureAwait(false);
			await birthdayTimer.DisposeAsync().ConfigureAwait(false);
		}

		private static async Task ProcessDatabaseUpdates(DataContext context, Update update, CancellationToken cancellationToken) {
			switch (update.Type) {
				case UpdateType.MyChatMember when update.MyChatMember.NewChatMember.Status is ChatMemberStatus.Member or ChatMemberStatus.Administrator: {
					Telegram.Bot.Types.Chat chat = update.MyChatMember.Chat;
					await UpdateChat(context, chat).ConfigureAwait(false);

					break;
				}
				case UpdateType.MyChatMember when update.MyChatMember.NewChatMember.Status is ChatMemberStatus.Kicked or ChatMemberStatus.Left: {
					Telegram.Bot.Types.Chat? chat = update.MyChatMember.Chat;
					Chat? dbChat = await context.Chats.Include(x => x.UserChats).FirstOrDefaultAsync(x => x.Id == chat.Id, cancellationToken).ConfigureAwait(false);
					if (dbChat != null) {
						dbChat.UserChats.Clear();
						context.Chats.Remove(dbChat);
					}

					break;
				}
				case UpdateType.ChatMember when update.ChatMember.NewChatMember.Status is ChatMemberStatus.Member or ChatMemberStatus.Administrator: {
					Telegram.Bot.Types.User? user = update.ChatMember.NewChatMember.User;
					Telegram.Bot.Types.Chat? chat = update.ChatMember.Chat;
					await UpdateUser(context, user).ConfigureAwait(false);
					await UpdateChat(context, chat).ConfigureAwait(false);
					await UpdateUserChat(context, chat, user).ConfigureAwait(false);

					break;
				}
				case UpdateType.ChatMember when update.ChatMember.NewChatMember.Status is ChatMemberStatus.Kicked or ChatMemberStatus.Left: {
					Telegram.Bot.Types.User? user = update.ChatMember.NewChatMember.User;
					Telegram.Bot.Types.Chat? chat = update.ChatMember.Chat;
					UserChat? participant = await context.UserChats.FindAsync(user.Id, chat.Id).ConfigureAwait(false);
					if (participant != null) {
						context.UserChats.Remove(participant);
					}

					await UpdateUser(context, user).ConfigureAwait(false);
					await UpdateChat(context, chat).ConfigureAwait(false);

					break;
				}
				case UpdateType.Message when update.Message.Chat.Type is ChatType.Private: {
					await UpdateUser(context, update.Message.From).ConfigureAwait(false);

					break;
				}
				case UpdateType.Message when update.Message.Chat.Type is ChatType.Supergroup or ChatType.Group: {
					await UpdateUser(context, update.Message.From).ConfigureAwait(false);
					await UpdateChat(context, update.Message.Chat).ConfigureAwait(false);
					await UpdateUserChat(context, update.Message.Chat, update.Message.From).ConfigureAwait(false);

					break;
				}
			}
		}

		private static async Task UpdateChat(DataContext context, Telegram.Bot.Types.Chat chat) {
			Chat? currentChat = await context.Chats.FindAsync(chat.Id).ConfigureAwait(false);
			if (currentChat != null) {
				currentChat.Name = chat.Title;
			} else {
				currentChat = new Chat {
					Id = chat.Id,
					Name = chat.Title
				};

				await context.Chats.AddAsync(currentChat).ConfigureAwait(false);
			}
		}

		private static async Task UpdateUser(DataContext context, Telegram.Bot.Types.User user) {
			User? currentUser = await context.Users.FindAsync(user.Id).ConfigureAwait(false);
			if (currentUser != null) {
				currentUser.FirstName = user.FirstName;
				currentUser.LastName = user.LastName;
			} else {
				currentUser = new User {
					Id = user.Id,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Username = user.Username
				};

				await context.Users.AddAsync(currentUser).ConfigureAwait(false);
			}
		}

		private static async Task UpdateUserChat(DataContext context, Telegram.Bot.Types.Chat chat, Telegram.Bot.Types.User user) {
			bool participantExists = await context.UserChats.AnyAsync(x => (x.ChatId == chat.Id) && (x.UserId == user.Id)).ConfigureAwait(false);
			if (!participantExists) {
				UserChat participant = new() {
					ChatId = chat.Id,
					UserId = user.Id,
					IsPublic = false
				};

				await context.UserChats.AddAsync(participant).ConfigureAwait(false);
			}
		}
	}
}
