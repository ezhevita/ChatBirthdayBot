using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatBirthdayBot;

public class DatabaseRepository : IRepository {
	private DatabaseRepository() { }

	public async Task<UserRecord?> GetUserByID(long id, CancellationToken cancellationToken) {
		DataContext context = new();
		await using (context) {
			return await context.Users.FindAsync(new object[] { id }, cancellationToken);
		}
	}

	public async Task SetUserBirthday(long userID, ushort? year, byte? month, byte? day, CancellationToken cancellationToken)
	{
		await using var context = new DataContext();

		var user = await context.Users.FindAsync(new object[] {userID}, cancellationToken: cancellationToken);
		if (user == null) {
			return;
		}

		user.BirthdayDay = day;
		user.BirthdayMonth = month;
		user.BirthdayYear = year;

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<List<UserRecord>> GetNearestBirthdaysForChat(long chatID, CancellationToken cancellationToken) {
		var date = DateTime.UtcNow.AddHours(3);
		var key = (ushort)((date.Month << 5) + date.Day);

		await using var context = new DataContext();

		return await context.UserChats
			.Include(x => x.User)
			.Where(x => (x.ChatId == chatID) && (x.User.BirthdayDay != null) && (x.User.BirthdayMonth != null))
			.Select(userChat => new { userChat, tempKey = userChat.User.BirthdayMonth * 32 + userChat.User.BirthdayDay })
			.OrderBy(x => x.tempKey < key ? x.tempKey + 12 * 32 : x.tempKey)
			.Select(x => x.userChat.User)
			.Take(10)
			.ToListAsync(cancellationToken: cancellationToken);
	}

	public async Task<List<UserRecord>> GetBirthdaysByDate(DateTime birthdayDate) {
		var month = birthdayDate.Month;
		var day = birthdayDate.Day;

		await using var context = new DataContext();

		return await context.Users
			.Where(user => (user.BirthdayDay == day) && (user.BirthdayMonth == month))
			.Where(user => user.Chats.Any())
			.Include(x => x.Chats)
			.ToListAsync();
	}

	public async Task ProcessDatabaseUpdates(Update update, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(update);
		await using var context = new DataContext();

		switch (update.Type) {
			case UpdateType.MyChatMember when update.MyChatMember!.NewChatMember.Status is ChatMemberStatus.Member or ChatMemberStatus.Administrator: {
				var chat = update.MyChatMember.Chat;
				await UpdateChat(context, chat);

				break;
			}
			case UpdateType.MyChatMember when update.MyChatMember.NewChatMember.Status is ChatMemberStatus.Kicked or ChatMemberStatus.Left: {
				var chat = update.MyChatMember.Chat;
				var dbChat = await context.Chats
					.Include(x => x.UserChats)
					.FirstOrDefaultAsync(x => x.Id == chat.Id, cancellationToken);

				if (dbChat != null) {
					dbChat.UserChats.Clear();
					context.Chats.Remove(dbChat);
				}

				break;
			}
			case UpdateType.ChatMember when update.ChatMember!.NewChatMember.Status is ChatMemberStatus.Member or ChatMemberStatus.Administrator: {
				var user = update.ChatMember.NewChatMember.User;
				var chat = update.ChatMember.Chat;
				await UpdateUser(context, user);
				await UpdateChat(context, chat);
				await UpdateUserChat(context, chat, user);

				break;
			}
			case UpdateType.ChatMember when update.ChatMember.NewChatMember.Status is ChatMemberStatus.Kicked or ChatMemberStatus.Left: {
				var user = update.ChatMember.NewChatMember.User;
				var chat = update.ChatMember.Chat;
				var participant = await context.UserChats.FindAsync(new object[] { user.Id, chat.Id }, cancellationToken);
				if (participant != null) {
					context.UserChats.Remove(participant);
				}

				await UpdateUser(context, user);
				await UpdateChat(context, chat);

				break;
			}
			case UpdateType.Message when update.Message!.Chat.Type is ChatType.Private: {
				await UpdateUser(context, update.Message.From!);

				break;
			}
			case UpdateType.Message when update.Message.Chat.Type is ChatType.Supergroup or ChatType.Group: {
				await UpdateUser(context, update.Message.From!);
				await UpdateChat(context, update.Message.Chat);
				await UpdateUserChat(context, update.Message.Chat, update.Message.From!);

				break;
			}
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public static async Task<DatabaseRepository> CreateAsync() {
		await using (var context = new DataContext()) {
			await context.Database.MigrateAsync();
		}

		return new DatabaseRepository();
	}

	private static async Task UpdateChat(DataContext context, Chat chat) {
		var currentChat = await context.Chats.FindAsync(chat.Id);
		if (currentChat != null) {
			currentChat.Name = chat.Title;
		} else {
			currentChat = new ChatRecord {
				Id = chat.Id,
				Name = chat.Title
			};

			context.Chats.Add(currentChat);
		}
	}

	private static async Task UpdateUser(DataContext context, User user) {
		var currentUser = await context.Users.FindAsync(user.Id);
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

			context.Users.Add(currentUser);
		}
	}

	private static async Task UpdateUserChat(DataContext context, Chat chat, User user) {
		var participantExists = await context.UserChats.AnyAsync(x => (x.ChatId == chat.Id) && (x.UserId == user.Id));
		if (!participantExists) {
			UserChat participant = new() {
				ChatId = chat.Id,
				UserId = user.Id,
				IsPublic = false
			};

			context.UserChats.Add(participant);
		}
	}
}
