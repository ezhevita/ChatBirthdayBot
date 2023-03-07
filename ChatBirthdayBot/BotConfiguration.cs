namespace ChatBirthdayBot;

public record BotConfiguration
{
	public string Token { get; set; } = null!;
	public long UserOwnerId { get; set; }
}
