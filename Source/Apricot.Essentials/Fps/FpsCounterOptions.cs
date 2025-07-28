namespace Apricot.Essentials.Fps;

public class FpsCounterOptions
{
    /// <summary>
    /// Would print average FPS after measuring how muh time it took to process specified number of frames. 
    /// </summary>
    public ulong MeasureFramesCount { get; set; } = 400;
}
