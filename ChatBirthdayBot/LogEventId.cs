namespace ChatBirthdayBot;

public enum LogEventId
{
	None,
	PollingErrorOccurred = 10000,
	HandlingErrorOccurred,
	RespondErrorOccurred,
	PostErrorOccurred,
	PrivateCommandMessageReceived,
	ChatCommandMessageReceived,
	MyChatMemberUpdateReceived,
	ChatMemberUpdateReceived,
	ChatMembersRemoved,
	ChatMemberCheckFailed
}
