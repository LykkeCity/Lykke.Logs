# Lykke.Logs

Lykke Implementation of logging system

[Nuget](https://www.nuget.org/packages/Lykke.Logs/)

## Overview

Starting from the version [5.0.0](https://github.com/LykkeCity/Lykke.Logs/releases/tag/5.0.0) Lykke logging system is built atop of [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1&tabs=aspnetcore2x), but main stuff of MS implementation is hidden from the app developer. This made to support some Lykke specific requirements and make API more narrow to facilitate developers decisions.

### Common stuff

## Usage

### Prerequisites

* You need to define ```ENV_INFO``` environment variable on your local machine (and on any machine, where your service will run too). It's recommended to define this variable at the OS user level (on your local machine) to spread out this variable across all of the projects. Of course, you could use ```launchsettings.json``` as well, if you wish. It's prefered if you'll specify your Lykke email name as the value. This will help another developers to determine who runs service locally.

### Initialization 

#### Using ```Lykke.Sdk```

Add following code to the ```IServiceProvider ConfigureServices(IServiceCollection services)``` method in the ```Startup.cs```:

```c#

return services.BuildServiceProvider<AppSettings>(options =>
{
    options.ApiTitle = "YourServiceName API";
    options.Logs = logs =>
    {
        // This is required configuration:
        
        logs.AzureTableName = "YourServiceNameLog";
        logs.AzureTableConnectionStringResolver = settings => settings.LykkeServiceService.Db.LogsConnString;

        // This is optional extended configuration:
        
        logs.Extended = extendedLogs =>
        {
            extendedLogs.ConfigureConsole = consoleLogs =>
            {
                consoleLogs.IncludeScopes = true;
            };

            extendedLogs.ConfigureAzureTable = azureTableLogs =>
            {
                azureTableLogs.BatchSizeThreshold = 500;
                azureTableLogs.MaxBatchLifetime = TimeSpan.FromSeconds(1);
            };

            extendedLogs.ConfigureEssentialSlackChannels = essentialSlackChannelsLogs =>
            {
                essentialSlackChannelsLogs.SpamGuard.SetMutePeriod(LogLevel.Error, TimeSpan.FromMinutes(5));
                essentialSlackChannelsLogs.SpamGuard.DisableGuarding();
            };

            extendedLogs.AddAdditionalSlackChannel("LykkeServiceImportantChannel", channelOptions =>
            {
                channelOptions.MinLogLevel = LogLevel.Warning;
                channelOptions.SpamGuard.SetMutePeriod(LogLevel.Error, TimeSpan.FromMinutes(1));
            });

            extendedLogs.AddAdditionalSlackChannel("LykkeServiceVerboseChannel", channelOptions =>
            {
                channelOptions.MinLogLevel = LogLevel.Information;
                channelOptions.SpamGuard.DisableGuarding();
            });
        };
    };
});        
```
Let's analyze this code. Among other, this code registers and configures logging system services, which you could use in your code. It adds essential loggers, that you can't remove and optionally you could add and configure additional loggers.

Essential loggers are:

* Console logger.
* Azure Table Storage logger.
* Essential Slack channels logger, which logs warnings, errors and critical entries to the ```sys-warnings``` and ```app-errors``` Slack channels.

Additional logger is:

* Additional Slack channel logger, which logs entries with log level higher than specified in configuration to the specific Slack channel.

Required configuration:

* ```logs.AzureTableName``` you should use it to specify table name where Azure Table logger will write its entries.
* ```logs.AzureTableConnectionStringResolver``` you should use it to specify connection string ```IReloadingManager``` for Azure Table logger.

Extended configuration:

* ```extendedLogs.ConfigureConsole``` allows you to configure Console logger. Avialable options:
  * ```consoleLogs.IncludeScopes``` allows you enable or disable [log scopes](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1&tabs=aspnetcore2x#log-scopes) in the Console logger.
* ```extendedLogs.ConfigureAzureTable``` **TODO**
* ```extendedLogs.ConfigureEssentialSlackChannels``` **TODO**
* ```extendedLogs.AddAdditionalSlackChannel``` allows you to add additional Slack channel logger.
  * ```channelOptions.MinLogLevel``` allows you to specify log level below which entries will be not logged to the given Slack channel. By default it's ```Information```.
  * ```channelOptions.SpamGuard``` allows to configure spam guard for the given Slack channel.
    * ```channelOptions.SpamGuard.SetMutePeriod``` allows you to specify mute period for the specific log level. By default it's ```1``` minute for ```Information```, ```Warning``` and ```Error``` levels.
    * ```channelOptions.SpamGuard.DisableGuarding``` allows you to disable spam guard at all. By default spam guard is enabled.
  
#### Using ```AddLykkeLogging``` extension

#### Custom

### In tests

### Obtaining ```ILog```

```
public class MyService
{
    private readonly ILog _log;
    
    public MyService(ILogFactory logFactory)
    {
        _log = logFactory.CreateLog(this);
    }
}
```

```
public class MyService
{
    private readonly ILog _log;
    
    public MyService(ILogFactory logFactory, string serviceInstanceName)
    {
        _log = logFactory.CreateLog(this, serviceInstanceName);
    }
}
```

### Log levels

Refer to the [```LogLevel```](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.logLevel?view=aspnetcore-2.1) documentation to learn when to use particular log level.

### Logging

### Health monitoring