using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ChatBirthdayBot.Database;
using ChatBirthdayBot.Localization;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

namespace ChatBirthdayBot {
	internal static class Program {
		private static readonly HashSet<UpdateType> AllowedUpdateTypes = new() { UpdateType.Message, UpdateType.ChatMember, UpdateType.MyChatMember };
		private static TelegramBotClient Bot = null!;
		private static readonly Timer CacheCleaner = new(CleanCache, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
		private static readonly ConcurrentDictionary<long, DateTime> LastChatSentMessage = new();
		private static readonly ConcurrentDictionary<long, int> LastSentBirthdaysMessage = new();
		private static IRepository Repository = null!;
		private static readonly SemaphoreSlim ShutdownSemaphore = new(0, 1);
		private static readonly CultureInfo RussianCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("ru-RU");

		private static int AgeFromDate(DateTime birthdate) {
			DateTime today = DateTime.Today;
			byte age = (byte) (today.Year - birthdate.Year);
			if (birthdate.Date > today.AddYears(-age)) {
				age--;
			}

			return age;
		}

		private static void CleanCache(object? state) {
			LastSentBirthdaysMessage.Clear();
			LastSentBirthdaysMessage.Clear();
		}

		private static async Task CheckBirthdays() {
			DateTime currentDate = DateTime.UtcNow.Date;

			List<UserRecord> todayBirthdays = await Repository.GetBirthdaysByDate(currentDate).ConfigureAwait(false);

			Dictionary<ChatRecord, List<UserRecord>> dictionary = todayBirthdays
				.Select(user => user.Chats.Select(chat => new { chat, user }))
				.SelectMany(x => x)
				.GroupBy(x => x.chat)
				.ToDictionary(x => x.Key, x => x.Select(y => y.user).ToList());

			foreach ((ChatRecord chat, List<UserRecord> users) in dictionary) {
				IEnumerable<string> usernamesToPost = users.Select(x => $"<a href=\"tg://user?id={x.Id}\">{Escape(x.FirstName)}</a>");

				try {
					Message message = await Bot.SendTextMessageAsync(chat.Id, string.Join(", ", usernamesToPost) + " - с днём рождения!", ParseMode.Html).ConfigureAwait(false);
					await Bot.PinChatMessageAsync(chat.Id, message.MessageId, false).ConfigureAwait(false);
				} catch (Exception e) {
					Console.WriteLine(e);
				}
			}
		}

		private static string Escape(string message) => HttpUtility.HtmlEncode(message);

		private static Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
			Console.WriteLine(exception);
			Bot.StartReceiving(new DefaultUpdateHandler(HandleUpdate, HandleError), cancellationToken: cancellationToken);
			return Task.CompletedTask;
		}

		private static void AppendUser(StringBuilder stringBuilder, User user) {
			stringBuilder.Append(user.Id);
			stringBuilder.Append(" (");
			if (!string.IsNullOrEmpty(user.Username)) {
				stringBuilder.Append(user.Username);
			} else {
				stringBuilder.Append(user.FirstName);
				if (!string.IsNullOrEmpty(user.LastName)) {
					stringBuilder.Append(' ');
					stringBuilder.Append(user.LastName);
				}
			}
						
			stringBuilder.Append(')');
		}
		
		private static void LogUpdate(Update update) {
			StringBuilder logMessageBuilder = new();
			logMessageBuilder.Append(update.Type.ToString());
			logMessageBuilder.Append('|');

			switch (update.Type) {
				case UpdateType.Message:
					Message message = update.Message!;

					logMessageBuilder.Append(message.Type.ToString());
					logMessageBuilder.Append('|');
					logMessageBuilder.Append(message.Chat.Type.ToString());
					if (message.Chat.Type != ChatType.Private) {
						logMessageBuilder.Append('|');
						logMessageBuilder.Append(message.Chat.Id);
						logMessageBuilder.Append(" (");
						logMessageBuilder.Append(message.Chat.Title);
						logMessageBuilder.Append(')');
					}

					User? from = message.From;
					if (from != null) {
						logMessageBuilder.Append('|');
						AppendUser(logMessageBuilder, from);
					}

					if (message.Type == MessageType.Text) {
						logMessageBuilder.Append('|');
						logMessageBuilder.Append(message.Text);
					}
					
					break;
				case UpdateType.ChatMember:
					ChatMemberUpdated chatMember = update.ChatMember!;

					logMessageBuilder.Append(chatMember.Chat.Id);
					logMessageBuilder.Append(" (");
					logMessageBuilder.Append(chatMember.Chat.Title);
					logMessageBuilder.Append(")|");
					logMessageBuilder.Append(chatMember.NewChatMember.Status.ToString());
					logMessageBuilder.Append('|');
					AppendUser(logMessageBuilder, chatMember.NewChatMember.User);

					break;
				case UpdateType.MyChatMember:
					ChatMemberUpdated myChatMember = update.MyChatMember!;
					
					logMessageBuilder.Append(myChatMember.Chat.Id);
					logMessageBuilder.Append(" (");
					logMessageBuilder.Append(myChatMember.Chat.Title);
					logMessageBuilder.Append(")|");
					logMessageBuilder.Append(myChatMember.NewChatMember.Status.ToString());

					break;
			}

			Console.WriteLine(logMessageBuilder.ToString());
		}
		
		private static async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
			if (!AllowedUpdateTypes.Contains(update.Type)) {
				return;
			}

			LogUpdate(update);
			await Repository.ProcessDatabaseUpdates(update, cancellationToken).ConfigureAwait(false);

			if ((update.Type != UpdateType.Message) || (update.Message!.Type != MessageType.Text)) {
				return;
			}

			Message? message = update.Message;
			if (message == null) {
				return;
			}

			string? messageText = message.Text;
			if (string.IsNullOrEmpty(messageText) || (messageText[0] != '/')) {
				return;
			}

			if (message.From == null) {
				return;
			}

			Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = message.From.LanguageCode switch {
				null or "" or "ru" or "uk" or "be" => RussianCulture,
				_ => CultureInfo.GetCultureInfoByIetfLanguageTag(message.From.LanguageCode)
			};

			string text = "";
			if (LastChatSentMessage.TryGetValue(message.Chat.Id, out DateTime lastSentMessage) && (lastSentMessage.AddSeconds(3) > message.Date)) {
				return;
			}

			LastChatSentMessage[message.Chat.Id] = message.Date;

			try {
				var currentUser = await Repository.GetUserByID(message.From.Id, cancellationToken).ConfigureAwait(false);
				if (currentUser == null) {
					return;
				}

				string[] args = messageText.ToUpperInvariant().Split(new[] { ' ', '@' }, StringSplitOptions.RemoveEmptyEntries);
				switch (args[0].ToUpperInvariant()) {
					case "/BIRTHDAYS" when message.Chat.Type is ChatType.Group or ChatType.Supergroup: {
						if (LastSentBirthdaysMessage.TryGetValue(message.Chat.Id, out int messageID)) {
							_ = Task.Run(async () => {
								try {
									await Bot.DeleteMessageAsync(message.Chat.Id, messageID, cancellationToken).ConfigureAwait(false);
								} catch {
									// ignored
								}
							}, cancellationToken);
						}

						List<UserRecord> birthdays = await Repository.GetNearestBirthdaysForChat(message.Chat.Id, cancellationToken).ConfigureAwait(false);

						text = string.Join(
							'\n',
							birthdays.Select(
								x => {
									DateTime birthdayDate = new(x.BirthdayYear ?? 0004, x.BirthdayMonth!.Value, x.BirthdayDay!.Value);

									return $"<b>{birthdayDate.ToString("d MMM", CultureInfo.CurrentCulture)}</b> — {Escape(x.FirstName)}{(x.LastName != null ? " " + Escape(x.LastName) : "")}{(x.BirthdayYear != null ? $" <b>({AgeFromDate(birthdayDate) + 1})</b>" : "")}";
								}
							)
						);

						break;
					}
					case "/BIRTHDAY": {
						if (message.Chat.Type != ChatType.Private) {
							break;
						}

						if (currentUser is { BirthdayDay: not null, BirthdayMonth: not null }) {
							DateTime date = new(currentUser.BirthdayYear ?? 0004, currentUser.BirthdayMonth.Value, currentUser.BirthdayDay.Value);
							text = string.Format(
								CultureInfo.CurrentCulture, Lines.BirthdayDate, date.Year == 0004
									? date.ToString("M", CultureInfo.CurrentCulture)
									: date.ToLongDateString().Replace(CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(date.DayOfWeek), "").TrimStart(',', ' ').TrimEnd('.')
							);
						} else {
							text = Lines.BirthdayNotSet;
						}

						break;
					}

					case "/SETBIRTHDAY" when (args.Length > 1) && message.Chat.Type is ChatType.Private: {
						string dateText = args[1];
						if (!DateTime.TryParseExact(dateText, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime birthdayDate)) {
							if (!DateTime.TryParseExact(dateText + "-0004", "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthdayDate)) {
								text = Lines.BirthdaySetFailed;

								break;
							}
						} else {
							if ((birthdayDate.AddYears(1) > DateTime.UtcNow.Date) || (birthdayDate.Year < 1900)) {
								text = Lines.BirthdaySetFailed;
								break;
							}
						}

						await Repository.SetUserBirthday(currentUser.Id, birthdayDate.Year == 0004 ? null : (ushort?)birthdayDate.Year, (byte)birthdayDate.Month, (byte)birthdayDate.Day, cancellationToken).ConfigureAwait(false);

						text = Lines.BirthdaySetSuccessfully;

						break;
					}

					case "/REMOVEBIRTHDAY" when message.Chat.Type is ChatType.Private: {
						if (currentUser.BirthdayDay != null) {
							await Repository.SetUserBirthday(currentUser.Id, null, null, null, cancellationToken).ConfigureAwait(false);

							text = Lines.BirthdayRemoved;
						} else {
							text = Lines.BirthdayNotSet;
						}

						break;
					}
					default:
						if (message.Chat.Type is ChatType.Private) {
							goto case "/BIRTHDAY";
						}

						break;
				}
			} finally {
				if (!string.IsNullOrEmpty(text)) {
					Message? sentMessage = null;
					try {
						sentMessage = await Bot.SendTextMessageAsync(message.Chat.Id, text, ParseMode.Html, replyToMessageId: message.MessageId, cancellationToken: cancellationToken).ConfigureAwait(false);
					} catch (Exception e) {
						Console.WriteLine(e);
					}

					if ((sentMessage != null) && (message.Text?.StartsWith("/birthdays", StringComparison.OrdinalIgnoreCase) == true)) {
						LastSentBirthdaysMessage[message.Chat.Id] = sentMessage.MessageId;
					}
				}
			}
		}

		private static async void CheckBirthdaysTimer(object? state) {
			await CheckBirthdays().ConfigureAwait(false);
		}

		private static async Task Main() {
			Bot = new TelegramBotClient(await File.ReadAllTextAsync("token.txt").ConfigureAwait(false));
			Timer birthdayTimer = new(
				CheckBirthdaysTimer,
				null,
				DateTime.Today.AddDays(1).AddHours(3) - DateTime.UtcNow,
				TimeSpan.FromDays(1)
			);

			Repository = await DatabaseRepository.CreateAsync().ConfigureAwait(false);

			Bot.StartReceiving(new DefaultUpdateHandler(HandleUpdate, HandleError));

			await CheckBirthdays().ConfigureAwait(false);
			await ShutdownSemaphore.WaitAsync().ConfigureAwait(false);
			await birthdayTimer.DisposeAsync().ConfigureAwait(false);
			await CacheCleaner.DisposeAsync().ConfigureAwait(false);
		}
	}
}
