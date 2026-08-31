[![](https://img.shields.io/nuget/v/Soenneker.Utils.Logger.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Utils.Logger/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.logger/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.logger/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Utils.Logger.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Utils.Logger/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.logger/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.logger/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Utils.Logger
Builds cached Microsoft `ILogger<T>` instances backed by Serilog's global logger and exposes a shared runtime level switch.

## Installation

```bash
dotnet add package Soenneker.Utils.Logger
```

## Quick start

```csharp
using Soenneker.Utils.Logger;
```

Call the static `LoggerUtil` methods directly; no dependency-injection registration is required.

## Configure Serilog before creating loggers

```csharp
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Soenneker.Utils.Logger;

LoggingLevelSwitch levelSwitch = LoggerUtil.GetSwitch();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(levelSwitch)
    .WriteTo.Console()
    .CreateLogger();

ILogger<Worker> logger = LoggerUtil.BuildLogger<Worker>();
```

The console call requires the corresponding Serilog sink package. Configure `Log.Logger` before
the first call to `BuildLogger` or `Init`: the utility's factory captures that logger and does not
own or dispose it. Application shutdown remains responsible for flushing and closing Serilog.

## Change the level at runtime

```csharp
LoggerUtil.SetLogLevel(LogEventLevel.Warning);

// Or resolve the configured level through Soenneker's configuration logging extension:
LoggerUtil.SetLogLevelFromConfig(configuration);
```

Level changes affect a logger only when its Serilog configuration uses the shared switch returned
by `GetSwitch`, as in the setup above. The switch begins at `Verbose`; sinks and additional Serilog
overrides can still filter events more restrictively.

`BuildLogger<T>` creates a Microsoft `ILogger<T>` with category `T` through the cached factory.
`Init` is optional because `BuildLogger` initializes the factory on first use.
