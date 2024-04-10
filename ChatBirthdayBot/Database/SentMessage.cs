using System;

namespace ChatBirthdayBot.Database;

public class SentMessage
{
	public long ChatId { get; init; }
	public int MessageId { get; init; }
	public DateTime SendDateUtc { get; init; }

	public ChatRecord Chat { get; init; }
}
