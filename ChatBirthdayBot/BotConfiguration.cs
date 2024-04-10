namespace ChatBirthdayBot;

public record BotConfiguration
{
	public string Token { get; init; } = null!;
	public long UserOwnerId { get; init; }
}
