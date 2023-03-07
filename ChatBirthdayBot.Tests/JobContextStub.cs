using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Quartz;

namespace ChatBirthdayBot.Tests;

[SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
public class JobContextStub : IJobExecutionContext
{
	public void Put(object key, object objectValue)
	{
		throw new NotImplementedException();
	}

	public object? Get(object key) => throw new NotImplementedException();

	public IScheduler Scheduler { get; } = null!;
	public ITrigger Trigger { get; } = null!;
	public ICalendar? Calendar { get; } = null!;
	public bool Recovering { get; }
	public TriggerKey RecoveringTriggerKey { get; } = null!;
	public int RefireCount { get; }
	public JobDataMap MergedJobDataMap { get; } = null!;
	public IJobDetail JobDetail { get; } = null!;
	public IJob JobInstance { get; } = null!;
	public DateTimeOffset FireTimeUtc { get; }
	public DateTimeOffset? ScheduledFireTimeUtc { get; set; } = null!;
	public DateTimeOffset? PreviousFireTimeUtc { get; } = null!;
	public DateTimeOffset? NextFireTimeUtc { get; } = null!;
	public string FireInstanceId { get; } = null!;
	public object? Result { get; set; } = null!;
	public TimeSpan JobRunTime { get; }
	public CancellationToken CancellationToken { get; }
}
