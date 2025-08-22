namespace Apricot.Jobs.Containers;

public class RingBuffer<T>(int initialLength = 32)
{
    private T[] _buffer = new T[initialLength];

    public T this[int index]
    {
        get => _buffer[index % _buffer.Length];
        set => _buffer[index % _buffer.Length] = value;
    }

    public int Length => _buffer.Length;

    public void Grow() => Array.Resize(ref _buffer, _buffer.Length * 2);
}
