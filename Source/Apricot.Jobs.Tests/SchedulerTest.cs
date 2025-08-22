namespace Apricot.Jobs.Tests;

[TestFixture]
public class SchedulerTest
{
    private IScheduler _scheduler;

    [SetUp]
    public void SetUp()
    {
        _scheduler = new Scheduler(4);
    }

    [Test]
    public void ScheduleOnMainThread()
    {
        var currentThread = Environment.CurrentManagedThreadId;
        var runnedOnThread = currentThread - 1; // to ensure inequality

        _scheduler.Schedule(() => runnedOnThread = Environment.CurrentManagedThreadId, true);
        _scheduler.RunMainThread();

        Assert.That(currentThread, Is.EqualTo(runnedOnThread));
    }
}
