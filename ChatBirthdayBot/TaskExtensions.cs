using System.Threading.Tasks;

namespace ChatBirthdayBot;

public static class TaskExtensions
{
	public static void RunInBackgroundSuppressingExceptions(this Task task)
	{
		Task.Run(
			async () =>
			{
				try
				{
					await task;
				}
				catch
				{
					// ignored
				}
			});
	}
}
