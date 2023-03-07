namespace ChatBirthdayBot;

public enum LogEventId
{
	None = 0,
	PollingErrorOccurred,
	HandlingErrorOccurred,
	RespondErrorOccurred,
	ExecutingPostBirthdaysJobForChat,
	PostErrorOccurred,
	PrivateCommandMessageReceived,
	ChatCommandMessageReceived,
	MyChatMemberUpdateReceived,
	ChatMemberUpdateReceived
}
