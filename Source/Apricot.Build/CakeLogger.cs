using Apricot.Build.Tasks;
using Cake.Core;
using Cake.Core.Diagnostics;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Apricot.Build;

/// <summary>
/// <see cref="ILogger{TCategoryName}"/> implementation attached to cake logging system. Ignores scopes.
/// </summary>
public class CakeLogger<T>(ICakeContext context) : ILogger<T>
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.None) return;
        
        context.Log.Write(
            Verbosity.Normal,
            logLevel switch
            {
                LogLevel.Critical => Cake.Core.Diagnostics.LogLevel.Fatal,
                LogLevel.Error => Cake.Core.Diagnostics.LogLevel.Error,
                LogLevel.Warning => Cake.Core.Diagnostics.LogLevel.Warning,
                LogLevel.Information => Cake.Core.Diagnostics.LogLevel.Information,
                LogLevel.Debug => Cake.Core.Diagnostics.LogLevel.Verbose,
                LogLevel.Trace => Cake.Core.Diagnostics.LogLevel.Debug, // yeah, in cake it is opposite
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            },
            formatter(state, exception)
        );
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}
