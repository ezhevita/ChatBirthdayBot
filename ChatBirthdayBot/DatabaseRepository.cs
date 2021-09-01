using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot {
	public class DatabaseRepository : IRepository {
		private DatabaseRepository() { }

		public async Task<UserRecord?> GetUserByID(long id, CancellationToken cancellationToken) {
			DataContext context = new();
			await using (context.ConfigureAwait(false)) {
				return await context.Users.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
			}
		}

		public async Task SetUserBirthday(long userID, ushort? year, byte? month, byte? day, CancellationToken cancellationToken) {
			DataContext context = new();
			await using (context.ConfigureAwait(false)) {
				UserRecord? user = await context.Users.FindAsync(new object[] { userID }, cancellationToken).ConfigureAwait(false);
				if (user == null) {
					return;
				}

				user.BirthdayDay = day;
				user.BirthdayMonth = month;
				user.BirthdayYear = year;

				await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		public async Task<List<UserRecord>> GetNearestBirthdaysForChat(long chatID, CancellationToken cancellationToken) {
			DateTime date = DateTime.UtcNow.AddHours(3);
			ushort key = (ushort)((date.Month << 5) + date.Day);

			DataContext context = new();
			await using (context.ConfigureAwait(false)) {
				return await context.UserChats
					.Include(x => x.User)
					.Where(x => (x.ChatId == chatID) && (x.User.BirthdayDay != null) && (x.User.BirthdayMonth != null))
					.Select(userChat => new { userChat, tempKey = userChat.User.BirthdayMonth * 32 + userChat.User.BirthdayDay })
					.OrderBy(x => x.tempKey < key ? x.tempKey + 12 * 32 : x.tempKey)
					.Select(x => x.userChat.User)
					.Take(10)
					.ToListAsync(cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}
		}

		public async Task<List<UserRecord>> GetBirthdaysByDate(DateTime date) {
			DataContext context = new();

			byte month = (byte)date.Month;
			byte day = (byte)date.Day;

			await using (context.ConfigureAwait(false)) {
				return await context.Users
					.Where(user => (user.BirthdayDay == day) && (user.BirthdayMonth == month))
					.Where(user => user.Chats.Any())
					.Include(x => x.Chats)
					.ToListAsync()
					.ConfigureAwait(false);
			}
		}

		public async Task ProcessDatabaseUpdates(Update update, CancellationToken cancellationToken) {
			DataContext context = new();
			await using (context.ConfigureAwait(false)) {
				switch (update.Type) {
					case UpdateType.MyChatMember when update.MyChatMember!.NewChatMember.Status is ChatMemberStatus.Member or ChatMemberStatus.Administrator: {
						Chat chat = update.MyChatMember.Chat;
						await UpdateChat(context, chat).ConfigureAwait(false);

						break;
					}
					case UpdateType.MyChatMember when update.MyChatMember.NewChatMember.Status is ChatMemberStatus.Kicked or ChatMemberStatus.Left: {
						Chat chat = update.MyChatMember.Chat;
						ChatRecord? dbChat = await context.Chats
							.Include(x => x.UserChats)
							.FirstOrDefaultAsync(x => x.Id == chat.Id, cancellationToken)
							.ConfigureAwait(false);

						if (dbChat != null) {
							dbChat.UserChats.Clear();
							context.Chats.Remove(dbChat);
						}

						break;
					}
					case UpdateType.ChatMember when update.ChatMember!.NewChatMember.Status is ChatMemberStatus.Member or ChatMemberStatus.Administrator: {
						User user = update.ChatMember.NewChatMember.User;
						Chat chat = update.ChatMember.Chat;
						await UpdateUser(context, user).ConfigureAwait(false);
						await UpdateChat(context, chat).ConfigureAwait(false);
						await UpdateUserChat(context, chat, user).ConfigureAwait(false);

						break;
					}
					case UpdateType.ChatMember when update.ChatMember.NewChatMember.Status is ChatMemberStatus.Kicked or ChatMemberStatus.Left: {
						User user = update.ChatMember.NewChatMember.User;
						Chat chat = update.ChatMember.Chat;
						UserChat? participant = await context.UserChats.FindAsync(new object[] { user.Id, chat.Id }, cancellationToken).ConfigureAwait(false);
						if (participant != null) {
							context.UserChats.Remove(participant);
						}

						await UpdateUser(context, user).ConfigureAwait(false);
						await UpdateChat(context, chat).ConfigureAwait(false);

						break;
					}
					case UpdateType.Message when update.Message!.Chat.Type is ChatType.Private: {
						await UpdateUser(context, update.Message.From!).ConfigureAwait(false);

						break;
					}
					case UpdateType.Message when update.Message.Chat.Type is ChatType.Supergroup or ChatType.Group: {
						await UpdateUser(context, update.Message.From!).ConfigureAwait(false);
						await UpdateChat(context, update.Message.Chat).ConfigureAwait(false);
						await UpdateUserChat(context, update.Message.Chat, update.Message.From!).ConfigureAwait(false);

						break;
					}
				}

				await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		public static async Task<DatabaseRepository> CreateAsync() {
			DataContext context = new();
			await using (context.ConfigureAwait(false)) {
				await context.Database.MigrateAsync().ConfigureAwait(false);
			}

			return new DatabaseRepository();
		}

		private async Task UpdateChat(DataContext context, Chat chat) {
			ChatRecord? currentChat = await context.Chats.FindAsync(chat.Id).ConfigureAwait(false);
			if (currentChat != null) {
				currentChat.Name = chat.Title;
			} else {
				currentChat = new ChatRecord {
					Id = chat.Id,
					Name = chat.Title
				};

				await context.Chats.AddAsync(currentChat).ConfigureAwait(false);
			}
		}

		private async Task UpdateUser(DataContext context, User user) {
			UserRecord? currentUser = await context.Users.FindAsync(user.Id).ConfigureAwait(false);
			if (currentUser != null) {
				currentUser.FirstName = user.FirstName;
				currentUser.LastName = user.LastName;
			} else {
				currentUser = new UserRecord {
					Id = user.Id,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Username = user.Username
				};

				await context.Users.AddAsync(currentUser).ConfigureAwait(false);
			}
		}

		private async Task UpdateUserChat(DataContext context, Chat chat, User user) {
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
