using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Soenneker.Extensions.Configuration.Logging;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Soenneker.Utils.Logger;

/// <summary>
/// Provides shared Serilog logging infrastructure for applications that do not
/// rely exclusively on dependency injection.
/// </summary>
/// <remarks>
/// This utility exposes a single lazily-initialized <see cref="LoggingLevelSwitch"/>
/// and a cached <see cref="SerilogLoggerFactory"/> for creating
/// <see cref="Microsoft.Extensions.Logging.ILogger"/> instances.
///
/// <para>
/// Initialization is performed exactly once using reference publication under a lock.
/// After initialization, all hot paths are lock-free and allocation-free.
/// </para>
///
/// <para>
/// Prefer dependency injection where possible. This type exists primarily for
/// static, library, or early-startup logging scenarios.
/// </para>
/// </remarks>
public static class LoggerUtil
{
    private static readonly Lock _initLock = new();

    private static LoggingLevelSwitch? _loggingLevelSwitch;
    private static SerilogLoggerFactory? _factory;

    /// <summary>
    /// Ensures the logging infrastructure is initialized.
    /// </summary>
    /// <remarks>
    /// This method is safe to call multiple times and from multiple threads.
    /// Initialization occurs exactly once.
    ///
    /// <para>
    /// Initialization state is determined by reference publication
    /// (i.e. <see cref="_factory"/> being non-null), which is safely published
    /// via a lock. No volatile or atomic operations are required.
    /// </para>
    /// </remarks>
    public static void Init()
    {
        // Fast path: already initialized
        if (_factory is not null)
            return;

        lock (_initLock)
        {
            if (_factory is not null)
                return;

            _loggingLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);
            _factory = new SerilogLoggerFactory(Log.Logger, dispose: false);
        }
    }

    /// <summary>
    /// Creates a Microsoft <see cref="ILogger{T}"/> using the cached
    /// <see cref="SerilogLoggerFactory"/>.
    /// </summary>
    /// <typeparam name="T">The category type for the logger.</typeparam>
    /// <returns>An <see cref="ILogger{T}"/> instance.</returns>
    /// <remarks>
    /// This method is optimized for the steady state:
    /// after initialization, it performs only a single reference read
    /// and a logger creation call.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ILogger<T> BuildLogger<T>()
    {
        SerilogLoggerFactory? factory = _factory;

        if (factory is null)
        {
            Init();
            factory = _factory!;
        }

        return factory.CreateLogger<T>();
    }

    /// <summary>
    /// Gets the shared <see cref="LoggingLevelSwitch"/> instance.
    /// </summary>
    /// <returns>The initialized <see cref="LoggingLevelSwitch"/>.</returns>
    /// <remarks>
    /// The returned instance is shared across the application and can be used
    /// to dynamically adjust log levels at runtime.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LoggingLevelSwitch GetSwitch()
    {
        LoggingLevelSwitch? sw = _loggingLevelSwitch;

        if (sw is null)
        {
            Init();
            sw = _loggingLevelSwitch!;
        }

        return sw;
    }

    /// <summary>
    /// Resolves and applies the minimum log level from configuration.
    /// </summary>
    /// <param name="config">The configuration source.</param>
    /// <returns>The resolved <see cref="LogEventLevel"/>.</returns>
    /// <remarks>
    /// This method reads the configured log level
    /// and applies it to the shared <see cref="LoggingLevelSwitch"/>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LogEventLevel SetLogLevelFromConfig(IConfiguration config) =>
        SetLogLevel(config.GetLogEventLevel());

    /// <summary>
    /// Sets the minimum log level on the shared <see cref="LoggingLevelSwitch"/>.
    /// </summary>
    /// <param name="logEventLevel">The minimum log level to apply.</param>
    /// <returns>The applied <see cref="LogEventLevel"/>.</returns>
    /// <remarks>
    /// Changing the log level affects all loggers created from this utility
    /// immediately.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LogEventLevel SetLogLevel(LogEventLevel logEventLevel)
    {
        GetSwitch()
            .MinimumLevel = logEventLevel;
        return logEventLevel;
    }
}