namespace ChatBirthdayBot;

#pragma warning disable CA1008
public enum LogEventId
#pragma warning restore CA1008
{
	PollingErrorOccurred = 10000,
	HandlingErrorOccurred,
	RespondErrorOccurred,
	PostErrorOccurred,
	PrivateCommandMessageReceived,
	ChatCommandMessageReceived,
	MyChatMemberUpdateReceived,
	ChatMemberUpdateReceived,
	ChatMembersRemoved,
	ChatMemberCheckFailed,
	ChatMemberCheckBatchProcessed
}
