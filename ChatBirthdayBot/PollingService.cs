using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChatBirthdayBot;

public class PollingService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;

	public PollingService(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			using var scope = _serviceProvider.CreateScope();
			var receiver = scope.ServiceProvider.GetRequiredService<ReceiverService>();

			await receiver.ReceiveAsync(stoppingToken);
		}
	}
}
