using Microsoft.Extensions.DependencyInjection;

namespace ChatBirthdayBot.Commands;

public class StartCommand : BirthdayInfoCommand
{
	public StartCommand(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
	{
	}

	public override string CommandName => "start";
}
