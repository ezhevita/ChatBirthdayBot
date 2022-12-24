using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using Telegram.Bot.Types;

namespace ChatBirthdayBot;

public interface IRepository {
	Task<List<UserRecord>> GetBirthdaysByDate(DateTime date);
	Task<List<UserRecord>> GetNearestBirthdaysForChat(long chatID, CancellationToken cancellationToken);
	Task<UserRecord?> GetUserByID(long id, CancellationToken cancellationToken);
	Task ProcessDatabaseUpdates(Update update, CancellationToken cancellationToken);
	Task SetUserBirthday(long userID, ushort? year, byte? month, byte? day, CancellationToken cancellationToken);
}