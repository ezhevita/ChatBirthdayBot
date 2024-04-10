namespace ChatBirthdayBot.Database;

public class UserChat
{
	public long UserId { get; set; }
	public UserRecord User { get; set; } = null!;
	public long ChatId { get; set; }
	public ChatRecord Chat { get; set; } = null!;
	public bool IsPublic { get; set; }
}
