using System.Threading;
using System.Threading.Tasks;
using ChatBirthdayBot.Commands;
using ChatBirthdayBot.Database;
using ChatBirthdayBot.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChatBirthdayBot.Tests;

[TestClass]
public class BirthdayInfoCommandTests : BaseTestClass
{
	[TestMethod]
	public async Task ExecuteCommand_BirthdayIsSet()
	{
		await using var context = CreateContext();
		await using var serviceProvider = CreateServiceProvider(context);

		context.Users.Add(
			new UserRecord
			{
				Id = 1,
				FirstName = "test",
				BirthdayDay = 10,
				BirthdayMonth = 10,
				BirthdayYear = 2000
			});

		var command = new BirthdayInfoCommand(serviceProvider.GetRequiredService<IServiceScopeFactory>());

		var result = await command.ExecuteCommand(
			Mock.Of<ITelegramBotClient>(), new Message {From = new User {Id = 1}}, CancellationToken.None);

		Assert.AreEqual(string.Format(Lines.BirthdayDate, "10 October 2000"), result);
	}

	[TestMethod]
	public async Task ExecuteCommand_BirthdayIsSetWithoutYear()
	{
		await using var context = CreateContext();
		await using var serviceProvider = CreateServiceProvider(context);

		context.Users.Add(
			new UserRecord
			{
				Id = 1,
				FirstName = "test",
				BirthdayDay = 9,
				BirthdayMonth = 11
			});

		var command = new BirthdayInfoCommand(serviceProvider.GetRequiredService<IServiceScopeFactory>());

		var result = await command.ExecuteCommand(
			Mock.Of<ITelegramBotClient>(), new Message {From = new User {Id = 1}}, CancellationToken.None);

		Assert.AreEqual(string.Format(Lines.BirthdayDate, "November 09"), result);
	}

	[TestMethod]
	public async Task ExecuteCommand_BirthdayNotSet()
	{
		await using var context = CreateContext();
		await using var serviceProvider = CreateServiceProvider(context);

		context.Users.Add(
			new UserRecord
			{
				Id = 1,
				FirstName = "test"
			});

		var command = new BirthdayInfoCommand(serviceProvider.GetRequiredService<IServiceScopeFactory>());

		var result = await command.ExecuteCommand(
			Mock.Of<ITelegramBotClient>(), new Message {From = new User {Id = 1}}, CancellationToken.None);

		Assert.AreEqual(Lines.BirthdayNotSet, result);
	}
}
