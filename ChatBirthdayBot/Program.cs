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
					}
				);

			services.AddDbContext<DataContext>(options => options.UseSqlite("Data Source=data.db"));
			services.AddQuartz(x => x.UseMicrosoftDependencyInjectionJobFactory());
			services.AddQuartzHostedService(x => x.WaitForJobsToComplete = true);
			services.AddScoped<IUpdateHandler, UpdateHandler>();
			services.AddScoped<ReceiverService>();
			services.AddSingleton<BotUserData>();

			services.AddScoped<ICommand, BirthdayInfoCommand>();
			services.AddScoped<ICommand, CheckChatMembersCommand>();
			services.AddScoped<ICommand, ListBirthdaysCommand>();
			services.AddScoped<ICommand, RemoveBirthdayCommand>();
			services.AddScoped<ICommand, SetBirthdayCommand>();

			services.AddScoped<ICommandHandler, CommandHandler>();

			services.AddHostedService<PollingService>();
		}
	)
	.Build();

var schedulerFactory = host.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();

var job = JobBuilder.Create<PostBirthdaysJob>()
	.WithIdentity("checkBirthdays", "birthdayBot")
	.Build();

var trigger = TriggerBuilder.Create()
	.WithIdentity("hourlyCheckBirthday", "birthdayBot")
	.StartAt(DateBuilder.NextGivenMinuteDate(DateTimeOffset.UtcNow, 0))
	.WithSimpleSchedule(x => x.WithIntervalInHours(1))
	.Build();

await scheduler.ScheduleJob(job, trigger);

using (var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
	await scope.ServiceProvider.GetRequiredService<DataContext>().Database.MigrateAsync();
}

await host.RunAsync();
