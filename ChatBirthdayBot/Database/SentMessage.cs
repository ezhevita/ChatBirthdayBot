using System;

namespace ChatBirthdayBot.Database;

public class SentMessage
{
	public long ChatId { get; set; }
	public int MessageId { get; set; }
	public DateTime SendDateUtc { get; set; }

	public ChatRecord Chat { get; set; }
}
