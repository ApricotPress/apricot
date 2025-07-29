using System.Collections.Concurrent;

namespace Apricot.Jobs.Tests;

[TestFixture]
public class WorkStealingDequeTests
{
    [Test]
    public void PushPop_ShouldReturnPushedItem()
    {
        var deque = new WorkStealingDeque<int>();
        deque.PushBottom(42);

        var result = deque.TryPopBottom(out var item);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(item, Is.EqualTo(42));
        });
    }

    [Test]
    public void TryPopBottom_OnEmptyDeque_ShouldReturnFalse()
    {
        var deque = new WorkStealingDeque<int>();

        var result = deque.TryPopBottom(out var item);

        Assert.That(result, Is.False);
    }

    [Test]
    public void TrySteal_OnEmptyDeque_ShouldReturnFalse()
    {
        var deque = new WorkStealingDeque<int>();

        var result = deque.TrySteal(out var item);

        Assert.That(result, Is.False);
    }

    [Test]
    public void TrySteal_AfterPushBottom_ShouldReturnItem()
    {
        var deque = new WorkStealingDeque<int>();
        deque.PushBottom(7);

        var result = deque.TrySteal(out var item);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(item, Is.EqualTo(7));
        });
    }

    [Test]
    public void Length_ShouldReflectNumberOfItems()
    {
        var deque = new WorkStealingDeque<int>();

        Assert.That(deque.Length, Is.EqualTo(0));

        deque.PushBottom(1);
        deque.PushBottom(2);

        Assert.That(deque.Length, Is.EqualTo(2));

        deque.TryPopBottom(out _);

        Assert.Multiple(() => { Assert.That(deque.Length, Is.EqualTo(1)); });
    }

    [Test]
    public void MultipleStealers_ShouldStealAllItems()
    {
        var deque = new WorkStealingDeque<int>();
        const int itemCount = 1000;

        for (var i = 0; i < itemCount; i++)
            deque.PushBottom(i);

        var results = new ConcurrentBag<int>();
        var threads = new List<Thread>();

        for (var t = 0; t < 4; t++)
        {
            var thread = new Thread(() =>
            {
                while (deque.TrySteal(out var item))
                {
                    results.Add(item);
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads)
            thread.Join();

        Assert.That(results.Count, Is.EqualTo(itemCount));
    }

    [Test]
    public void PopAndSteal_ShouldNotOverlap()
    {
        const int itemCount = 1000;
        var deque = new WorkStealingDeque<int>();

        for (var i = 0; i < itemCount; i++)
            deque.PushBottom(i);

        var popResults = new ConcurrentBag<int>();
        var stealResults = new ConcurrentBag<int>();

        var stealer = new Thread(() =>
        {
            while (deque.TrySteal(out var item))
            {
                stealResults.Add(item);
            }
        });

        var popper = new Thread(() =>
        {
            while (deque.TryPopBottom(out var item))
            {
                popResults.Add(item);
            }
        });

        stealer.Start();
        popper.Start();

        stealer.Join();
        popper.Join();

        var total = popResults.Count + stealResults.Count;

        Assert.Multiple(() =>
        {
            Assert.That(total, Is.EqualTo(itemCount));
            Assert.That(popResults.Intersect(stealResults).Any(), Is.False);
        });
    }
}
