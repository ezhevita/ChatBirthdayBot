using System;
using ChatBirthdayBot;
using ChatBirthdayBot.Commands;
using ChatBirthdayBot.Database;
using ChatBirthdayBot.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Polling;

var host = Host.CreateDefaultBuilder(args)
	.ConfigureServices(
		(context, services) =>
		{
			services.Configure<BotConfiguration>(context.Configuration.GetSection("Bot"));

			services.AddHttpClient("telegram_bot_client")
				.AddTypedClient<ITelegramBotClient>(
					(httpClient, sp) =>
					{
						var botConfig = sp.GetRequiredService<IOptions<BotConfiguration>>().Value;
						var options = new TelegramBotClientOptions(botConfig.Token);

						return new TelegramBotClient(options, httpClient);
					});

			services.AddDbContext<DataContext>(options => options.UseSqlite("Data Source=data.db"));
			services.AddQuartz();
			services.AddQuartzHostedService(x => x.WaitForJobsToComplete = true);
			services.AddScoped<IUpdateHandler, UpdateHandler>();
			services.AddScoped<ReceiverService>();
			services.AddSingleton<BotUserData>();

			services.AddSingleton<ICommand, BirthdayInfoCommand>();
			services.AddSingleton<ICommand, ListBirthdaysCommand>();
			services.AddSingleton<ICommand, RemoveBirthdayCommand>();
			services.AddSingleton<ICommand, SetBirthdayCommand>();
			services.AddSingleton<ICommand, StartCommand>();

			services.AddScoped<ICommandHandler, CommandHandler>();

			services.AddHostedService<PollingService>();
		}
	)
	.Build();

var schedulerFactory = host.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();

const string QuartzGroupName = "birthdayBot";

await scheduler.ScheduleJob(CreateJob<PostBirthdaysJob>("checkBirthdays"), CreateTrigger("hourlyCheckBirthday", 1));
await scheduler.ScheduleJob(CreateJob<UnpinBirthdaysMessagesJob>("unpinMessages"), CreateTrigger("hourlyUnpinMessages", 1));
await scheduler.ScheduleJob(CreateJob<CheckChatMembersJob>("checkChatMembers"), CreateTrigger("dailyCheckChatMembers", 1));

using (var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
	await scope.ServiceProvider.GetRequiredService<DataContext>().Database.MigrateAsync();
}

await host.RunAsync();

return;

static IJobDetail CreateJob<T>(string name) where T : IJob =>
	JobBuilder.Create<T>()
		.WithIdentity(name, QuartzGroupName)
		.Build();

static ITrigger CreateTrigger(string name, int hours)
{
	return TriggerBuilder.Create()
		.WithIdentity(name, QuartzGroupName)
		.StartAt(DateBuilder.NextGivenMinuteDate(DateTimeOffset.UtcNow, 0))
		.WithSimpleSchedule(schedule => schedule
			.WithIntervalInHours(hours)
			.RepeatForever())
		.Build();
}
