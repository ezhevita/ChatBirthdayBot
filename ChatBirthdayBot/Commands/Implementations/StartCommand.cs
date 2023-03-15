using ChatBirthdayBot.Database;

namespace ChatBirthdayBot.Commands;

public class StartCommand : BirthdayInfoCommand
{
	public StartCommand(DataContext context) : base(context)
	{
	}

	public override string CommandName => "start";
}
