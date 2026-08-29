[![](https://img.shields.io/nuget/v/Soenneker.Utils.Logger.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Utils.Logger/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.logger/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.logger/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Utils.Logger.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Utils.Logger/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.logger/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.logger/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Utils.Logger
A useful utility library dealing with Serilog logging.

## Installation

```bash
dotnet add package Soenneker.Utils.Logger
```

## Quick start

```csharp
using Soenneker.Utils.Logger;
```

Call the static `LoggerUtil` methods directly; no dependency-injection registration is required.

## Common operations

- `Init()` - Ensures the logging infrastructure is initialized. This method is safe to call multiple times and from multiple threads.
- `BuildLogger()` - Creates `ILogger<T>` through a cached `SerilogLoggerFactory`.
- `GetSwitch()` - Gets the shared `LoggingLevelSwitch` instance. Returns the initialized `LoggingLevelSwitch`.
- `SetLogLevelFromConfig()` - Resolves and applies the minimum log level from configuration. Returns the resolved `LogEventLevel`.
- `SetLogLevel()` - Sets the minimum log level on the shared `LoggingLevelSwitch`. Returns the applied `LogEventLevel`. Changing the log level affects all loggers created from this utility immediately.
