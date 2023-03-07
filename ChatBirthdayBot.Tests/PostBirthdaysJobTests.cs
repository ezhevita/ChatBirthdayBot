using System;
using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Database;
using ChatBirthdayBot.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace ChatBirthdayBot.Tests;

[TestClass]
public class PostBirthdaysJobTests : BaseTestClass
{
	[TestMethod]
	public async Task BirthdayIsAtMidnightWithoutOffsetAndTimezone_Success()
	{
		var telegramClientMock = new Mock<ITelegramBotClient>(MockBehavior.Strict);
		var context = CreateContext();

		context.Users.Add(new UserRecord {BirthdayDay = 1, BirthdayMonth = 1, Id = 1, FirstName = "Test"});
		context.Chats.Add(new ChatRecord {Id = -100});
		context.UserChats.Add(new UserChat {UserId = 1, ChatId = -100});

		await context.SaveChangesAsync();

		var job = new PostBirthdaysJob(telegramClientMock.Object, context, NullLogger<PostBirthdaysJob>.Instance);
		telegramClientMock.Setup(
			x => x.MakeRequestAsync(
				It.Is<SendMessageRequest>(r => r.Text == "<a href=\"tg://user?id=1\">Test</a> — happy birthday!" && r.ChatId == -100),
				It.IsAny<CancellationToken>()
			)
		)
			.ReturnsAsync(new Message())
			.Verifiable();

		telegramClientMock.Setup(
				x => x.MakeRequestAsync(
					It.IsAny<PinChatMessageRequest>(), It.IsAny<CancellationToken>()
				)
			)
			.ReturnsAsync(true)
			.Verifiable();

		await job.Execute(new JobContextStub {ScheduledFireTimeUtc = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero)});

		telegramClientMock.Verify();
	}
}
