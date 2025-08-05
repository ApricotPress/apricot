namespace Apricot.Timing;

public class TimeController(ITime time) : ITimeController
{
    public void Tick() => time.Step();
}
