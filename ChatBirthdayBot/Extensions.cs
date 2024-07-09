using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ChatBirthdayBot;

public static class Extensions
{
	private static readonly SemaphoreSlim _saveChangesSemaphore = new(1, 1);
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

	public static async Task SaveChangesConcurrentAsync(this DbContext dbContext, CancellationToken cancellationToken)
	{
		await _saveChangesSemaphore.WaitAsync(cancellationToken);
		try
		{
			await dbContext.SaveChangesAsync(cancellationToken);
		} finally
		{
			_saveChangesSemaphore.Release();
		}
	}
}
