using System.Diagnostics.CodeAnalysis;

namespace Apricot.Jobs;

public class WorkStealingDeque<T>(int initialSize = 32)
{
    private int _top;
    private int _lastTop;
    private int _bottom;

    private readonly RingBuffer<T> _buffer = new(initialSize);

    public void PushBottom(T item)
    {
        var btm = Volatile.Read(ref _bottom);
        var capacity = btm - _lastTop;

        if (capacity >= _buffer.Length)
        {
            _lastTop = Volatile.Read(ref _top);
            var actualSize = btm - _lastTop;

            if (actualSize >= _buffer.Length)
            {
                _buffer.Grow();
            }
        }

        _buffer[btm] = item;

        Volatile.Write(ref _bottom, btm + 1);
    }

    public bool TryPopBottom([MaybeNullWhen(false)] out T item)
    {
        var btm = Volatile.Read(ref _bottom) - 1;
        Volatile.Write(ref _bottom, btm);

        var top = Volatile.Read(ref _top);

        if (btm - top < 0)
        {
            Volatile.Write(ref _bottom, top);
            item = default;
            return false;
        }

        var bottomItem = _buffer[btm];
        if (btm - top > 0)
        {
            item = bottomItem;
            // todo: check for memory leak?
            return true;
        }

        // checking if we won racing race condition
        if (!CasTop(top, top + 1))
        {
            // we lost race, therefore we are empty
            Volatile.Write(ref _bottom, top + 1);
            item = default;
            return false;
        }

        Volatile.Write(ref _bottom, top + 1);
        item = bottomItem;
        return true;
    }

    public bool TrySteal([MaybeNullWhen(false)] out T item)
    {
        var top = Volatile.Read(ref _top);
        var btm = Volatile.Read(ref _bottom);

        if (btm <= top)
        {
            item = default;
            return false;
        }

        var stolen = _buffer[top];

        if (!CasTop(top, top + 1))
        {
            item = default;
            return false;
        }

        item = stolen;
        return true;
    }

    private bool CasTop(int oldValue, int newValue) =>
        Interlocked.CompareExchange(ref _top, newValue, oldValue) == oldValue;

    public int Length => Volatile.Read(ref _bottom) - Volatile.Read(ref _top);
}
