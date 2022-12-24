#nullable disable

namespace ChatBirthdayBot.Database;

public class UserChat {
	public long UserId { get; set; }
	public UserRecord User { get; set; }
	public long ChatId { get; set; }
	public ChatRecord Chat { get; set; }
	public bool IsPublic { get; set; }
}