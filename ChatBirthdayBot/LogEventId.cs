namespace ChatBirthdayBot;

public enum LogEventId
{
	None = 0,
	PollingErrorOccurred,
	HandlingErrorOccurred,
	RespondErrorOccurred,
	PostErrorOccurred,
	PrivateCommandMessageReceived,
	ChatCommandMessageReceived,
	MyChatMemberUpdateReceived,
	ChatMemberUpdateReceived
}
