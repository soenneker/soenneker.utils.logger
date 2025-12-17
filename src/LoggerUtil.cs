using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Soenneker.Extensions.Configuration.Logging;

namespace Soenneker.Utils.Logger;

/// <summary>
/// A utility library dealing with Serilog logging infrastructure.
/// </summary>
/// <remarks>
/// Provides a shared <see cref="LoggingLevelSwitch"/> and a cached
/// <see cref="SerilogLoggerFactory"/> for creating Microsoft
/// <see cref="Microsoft.Extensions.Logging.ILogger"/> instances. Prefer dependency injection where possible.
/// </remarks>
public static class LoggerUtil
{
    private static readonly Lock _initLock = new();
    private static bool _initialized;
    private static LoggingLevelSwitch? _loggingLevelSwitch;
    private static SerilogLoggerFactory? _factory;

    /// <summary>
    /// Initializes the logging level switch and logger factory.
    /// Safe to call multiple times; initialization occurs only once.
    /// </summary>
    public static void Init()
    {
        if (Volatile.Read(ref _initialized))
            return;

        lock (_initLock)
        {
            if (_initialized)
                return;

            _loggingLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);
            _factory = new SerilogLoggerFactory(Log.Logger, dispose: false);

            Volatile.Write(ref _initialized, true);
        }
    }

    /// <summary>
    /// Creates a Microsoft <see cref="ILogger{T}"/> using the cached Serilog logger factory.
    /// Prefer dependency injection where possible.
    /// </summary>
    /// <typeparam name="T">The category type for the logger.</typeparam>
    /// <returns>An <see cref="ILogger{T}"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ILogger<T> BuildLogger<T>()
    {
        if (!Volatile.Read(ref _initialized))
            Init();

        return _factory!.CreateLogger<T>();
    }

    /// <summary>
    /// Gets the shared <see cref="LoggingLevelSwitch"/> instance.
    /// </summary>
    /// <returns>The initialized <see cref="LoggingLevelSwitch"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LoggingLevelSwitch GetSwitch()
    {
        if (!Volatile.Read(ref _initialized))
            Init();

        return _loggingLevelSwitch!;
    }

    /// <summary>
    /// Sets the minimum log level based on configuration values.
    /// </summary>
    /// <param name="config">The configuration source.</param>
    /// <returns>The resolved <see cref="LogEventLevel"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LogEventLevel SetLogLevelFromConfig(IConfiguration config) => SetLogLevel(config.GetLogEventLevel());

    /// <summary>
    /// Sets the minimum log level on the shared <see cref="LoggingLevelSwitch"/>.
    /// </summary>
    /// <param name="logEventLevel">The minimum log level to apply.</param>
    /// <returns>The applied <see cref="LogEventLevel"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LogEventLevel SetLogLevel(LogEventLevel logEventLevel)
    {
        GetSwitch()
            .MinimumLevel = logEventLevel;

        return logEventLevel;
    }
}