namespace ChatBirthdayBot;

public enum LogEventId
{
	None = 0,
	PollingErrorOccurred,
	HandlingErrorOccurred,
	RespondErrorOccurred,
	EvaluatingPostBirthdaysJobForChat,
	PostErrorOccurred,
	PrivateCommandMessageReceived,
	ChatCommandMessageReceived,
	MyChatMemberUpdateReceived,
	ChatMemberUpdateReceived
}
