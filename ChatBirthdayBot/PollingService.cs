using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatBirthdayBot;

public partial class PollingService : BackgroundService
{
	private readonly ILogger _logger;
	private readonly IServiceProvider _serviceProvider;

	public PollingService(IServiceProvider serviceProvider, ILogger<PollingService> logger)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
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
