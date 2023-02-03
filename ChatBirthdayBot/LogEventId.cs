namespace ChatBirthdayBot;

public enum LogEventId
{
	None = 0,
	PollingErrorOccurred,
	HandlingErrorOccurred,
	RespondErrorOccurred,
	PrivateCommandMessageReceived,
	ChatCommandMessageReceived,
	MyChatMemberUpdateReceived,
	ChatMemberUpdateReceived
}
