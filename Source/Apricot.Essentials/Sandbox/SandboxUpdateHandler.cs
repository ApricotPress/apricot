using Apricot.Lifecycle.TickHandlers;
using Apricot.Timing;
using Microsoft.Extensions.Logging;

namespace Apricot.Essentials.Sandbox;

public class SandboxUpdateHandler(ITime time, ILogger<SandboxUpdateHandler> logger) : IUpdateHandler
{
    private readonly List<float> _lastDeltas = new(); 
    
    public void Tick()
    {
        _lastDeltas.Add(time.Delta);
        
        if (_lastDeltas.Count > 100)
        {
            _lastDeltas.RemoveAt(0);
        }
        
        
        logger.LogInformation("Average FPS: {Average:F}", 1f / _lastDeltas.Average());
    }
}
