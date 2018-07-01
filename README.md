# Lykke.Logs

Lykke Implementation of logging system

[Nuget](https://www.nuget.org/packages/Lykke.Logs/)

## Overview

Starting from the version [5.0.0](https://github.com/LykkeCity/Lykke.Logs/releases/tag/5.0.0) Lykke logging system is built atop of [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1&tabs=aspnetcore2x), but main stuff of MS implementation is hidden from the app developer. This made to support some Lykke specific requirements and make API more narrow to facilitate developers decisions.

### Common stuff

**TODO**

## Usage

### Prerequisites

#### ENV_INFO
You need to define ```ENV_INFO``` environment variable on your local machine (and on any machine, where your service will run too). It's recommended to define this variable at the OS user level (on your local machine) to spread out this variable across all of the projects. Of course, you could use ```launchsettings.json``` as well, if you wish. It's prefered if you'll specify your Lykke email name as the value. This will help another developers to determine who runs service locally. 

For dev, test and prod environments you could specify, if your service is hosted in kubernetes and you don't need any custom name of app instance, you could add following to your ```deployment.yaml```:

```yaml
spec:
  template:
    spec:
      - name: container-name
        env:
        - name: ENV_INFO
          valueFrom:
            fieldRef:
              fieldPath: metadata.name
```

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
            extendedLogs.SetMinimumLevel(LogLevel.Trace);
            extendedLogs.AddFilter("Lykke.MyService", LogLevel.Debug);
            extendedLogs.AddFilter("Lykke.MyService.MyClass", LogLevel.Error);
        
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

* Filtering. 
  * ```extendedLogs.SetMinimumLevel(LogLevel.Trace)``` allows you to set minimal log level for the entire logging system. Default value is ```Information```.
  * There is a set of ```AddFilter``` method overlods, which you could use to filter out logged entries by component and log level. 
    * ```extendedLogs.AddFilter("Lykke.MyService", LogLevel.Debug)``` sets minimal log level for all classes in the ```Lykke.MyService``` namespace to the ```Debug``` - all entries below this level will be filtered out.
    * ```extendedLogs.AddFilter("Lykke.MyService.MyClass", logLevel => logLevel == LogLevel.Critical && logLevel == LogLevel.Debug)``` filters out all log entries except ```Critical``` and ```Debug``` log levels for the ```Lykke.MyService.MyClass``` class and its nesteded classes, if any.
    * There are another overloads of ```AddFilter``` method, explore them.
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

**TODO**

### Use configuration file

**TODO**

### In tests

**TODO**

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

```ILog``` itself has ```void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) where TState : LogEntryParameters``` method. This is most low level method and you unlikely ever need to use it. One level higher is extension method ```public static void Log(this ILog log, LogLevel logLevel, string callerFilePath, string process, int callerLineNumber, string message, object context, Exception exception, DateTime? moment)``` which is defined in the ``` Lykke.Common.Log.MicrosoftLoggingBasedLogExtensions``` class. This method is inteded to be used only by other extension methods, so you unlikely ever need to use it too.

What you have to use in your app code, is set of extension methods, defined in the ``` Lykke.Common.Log.MicrosoftLoggingBasedLogExtensions``` class. There are two overloads for each log level - one overload with implicit ```process``` value (where ```process``` parameter has defalt value of ```null```) and one with explicit ```process``` value (where ```process``` parameter has no default value). If overload with implicit ```process``` value is used, then caller method name will be used as the ```process``` value. These method names are:

* ```Trace```
* ```Debug```
* ```Info```
* ```Warning```
* ```Error```
* ```Critical```

Either ```message``` or ```exception``` should be specified for each of these methods. Empty or whitespace string message will be treated as absence of the value. Both ```message``` and ```exception``` could be specified for any of the methods.

```context``` could be either string or any object. String ```context``` will be passed as is, whilst any other object will be serialized to the ```Json``` using ```Json.Net``` serializer with following serializer settings:

```c#
new JsonSerializerSettings
{
    Formatting = Formatting.Indented,
    NullValueHandling = NullValueHandling.Include,
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
    Culture = CultureInfo.InvariantCulture
};
```

Never specify explicit values for the ```callerFilePath``` and ```callerLineNumber``` they marked with ```CallerFilePath``` and ```CallerLineNumber``` attributes respectively and will be filled by the compiler autmatically.

Examples:

```csharp
_log.Trace("Some very detailed message");
_log.Debug("This message could be useful to debug an issue", new { parameterA, PropertyC });
_log.Info("Very useful process", "The process is beign started", processParameters);
_log.Warning("Value can't be parsed, skipping", exception, value);
_log.Error(exception, "Can't proceed processing");
_log.Critical(exception, "Invalid configuration, app will shutdown");
```

### Health monitoring

To notify about app health changing you could inject ```IHealthNotifier``` into constructor of your class, where you need to notify about it. This interface has the only one method ```Notify``` where you could pass message string and optional context object. The message will be published to the Slack ```system-monitoring``` channel and to the ```Information``` level of the log. If you use ```Lykke.Sdk``` health notifier will be used to notify about app start and stop automatically. 

```c#
public class MyService
{
    private readonly IHealthNotifier _healthNotifier;
    
    public MyService(IHealthNotifier healthNotifier)
    {
        _healthNotifier = healthNotifier;
    }
    
    public void HandleSomethingReallyBad(ReallyBadEvent event)
    {
        _helathNotifier.Notify("Something really bad happened with app", event);
    }
}
```