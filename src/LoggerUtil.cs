using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Soenneker.Extensions.Configuration.Logging;

namespace Soenneker.Utils.Logger;

/// <summary>
/// A useful utility library dealing with Serilog logging
/// </summary>
public static class LoggerUtil
{
    private static LoggingLevelSwitch? _loggingLevelSwitch;

    public static void Init()
    {
        _loggingLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);
    }

    /// <summary>
    /// Uses the static Serilog Log.Logger, and returns Microsoft ILogger after building a new one. Avoid if you can, utilize DI!
    /// Serilog should be configured with applicable sinks before calling this
    /// </summary>
    public static ILogger<T> BuildLogger<T>()
    {
        return new SerilogLoggerFactory(Log.Logger).CreateLogger<T>();
    }

    public static LoggingLevelSwitch GetSwitch()
    {
        if (_loggingLevelSwitch == null)
            throw new NullReferenceException("Make sure to call Init() before getting the log level switch");

        return _loggingLevelSwitch;
    }

    public static LogEventLevel SetLogLevelFromConfig(IConfiguration config)
    {
        LogEventLevel switchLevel = config.GetLogEventLevel();

        return SetLogLevel(switchLevel);
    }

    public static LogEventLevel SetLogLevel(LogEventLevel logEventLevel)
    {
        _loggingLevelSwitch!.MinimumLevel = logEventLevel;

        return logEventLevel;
    }
}