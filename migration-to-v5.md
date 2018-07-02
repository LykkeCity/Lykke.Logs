# Overview

In the version [5.0.0](https://github.com/LykkeCity/Lykke.Logs/releases/tag/5.0.0), configuration and logging API is totally redesigned. Read full documentation [here](https://github.com/LykkeCity/Lykke.Logs/blob/master/README.md).

# Options

When you updates up to the v5.х from the previous versions, you have two options:

1. Just update nugets and leave your code as-is. In this case you'll see a lot of warnings about obsolete types and their members. If you don't want to migrate to the new logging API, you could just ignore this warnings. You don't need to read this article if you choose this option. But keep in mind, that legacy API will be removed in the next releases.
3. Update nugets, modify logging system initialization, update obtaining of the ```ILog``` in your classes, switch to the new logging methods everywhere. All these points are described below.

# Update nugets

If your service references any nugets listed below directly or transitevly, you have to update them to at least these versions:

- https://github.com/LykkeCity/CommonDotNetLibraries/releases/tag/7.0.1
- https://github.com/LykkeCity/Lykke.Logs/releases/tag/5.1.0
- https://github.com/LykkeCity/AzureStorage/releases/tag/8.6.0
- https://github.com/LykkeCity/AzureQueueIntegration/releases/tag/2.2.0
- https://github.com/LykkeCity/JobTriggers/releases/tag/2.2.0
- https://github.com/LykkeCity/Lykke.Common.ApiLibrary/releases/tag/1.9.0
- https://github.com/LykkeCity/Lykke.RabbitMqDotNetBroker/releases/tag/7.1.0
- https://github.com/LykkeCity/Lykke.RabbitMq.Azure/releases/tag/5.1.0
- https://github.com/LykkeCity/Lykke.Messaging/releases/tag/5.1.0
- https://github.com/LykkeCity/Lykke.Messaging/releases/tag/rabbitmq-2.1.0
- https://github.com/LykkeCity/MonitoringService/releases/tag/1.6.0-client
- https://github.com/LykkeCity/Lykke.Cqrs/releases/tag/4.8.0
- https://github.com/LykkeCity/Lykke.Sdk/releases/tag/3.0.1

Only common Lykke infrastructure nugets already have v5.x support. There are some infrastructure nuget and a lot of service client nuget are not support v5.x yet. If your service uses on of these nuget, then read [Client nugets](https://github.com/LykkeCity/Lykke.Logs/blob/master/migration-to-v5.md#Client%20nugets) section or contact [Konstantin Ryazantsev](https://t.me/KonstantinRyazantsev).

# Initialization

## Prerequesites

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

## Without Lykke.Sdk

1. Instead of ```CreateLogWithSlack``` method call in your ```Startup.ConfigureServices``` method, add call of the ```services.AddLykkeLogging``` method. Example:

```c#
services.AddLykkeLogging
(
    candlesHistory.ConnectionString(x => x.Db.LogsConnectionString),
    "CandlesHistoryServiceLogs",
    settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
    settings.CurrentValue.SlackNotifications.AzureQueue.QueueName,
    // Optional
    logging =>
    {
        // This is necessary, if your service uses additional personal slack channel:
        logging.AddAdditionalSlackChannel("Prices", options =>
        {
            // Optional: default is LogLevel.Information
            options.MinLogLevel = LogLevel.Debug;
            // Optional: by default is enabled
            options.SpamGuard.DisableGuarding();
        });
    }
);
```

2. Obtain ```ILog``` and ```IHealthNotifier``` instances for the ```Startup``` right after DI container building:

```c#
ApplicationContainer = builder.Build();

Log = ApplicationContainer.Resolve<ILogFactory>().CreateLog(this);
HealthNotifier = ApplicationContainer.Resolve<IHealthNotifier>();
```

3. Replace ```ILog.WriteMonitor```/```ILogWriteMonitoryAsync``` calls with ```HealthNotifier.Notify``` calls. ```component``` and ```process``` parameters should be just removed.

4. Remove unused ```CreateLogWithSlack``` method.

## Using Lykke.Sdk

**TODO**

## In tests

1. If you need just a stub of the ```ILogFactory```, you should use ```Lykke.Logs.EmptyLogFactory.Instance```.
2. If you want that your tests will output logs to the ```stdout```, you should create ```LogFactory``` with unbuffered console provider and dispose it when you don't need it anymore. Example:

```c#
    [TestFixture]
    public class CommandDispatcherTests : IDisposable
    {
        private readonly ILogFactory _logFactory;

        public CommandDispatcherTests()
        {
            _logFactory = LogFactory.Create().AddUnbufferedConsole();
        }

        public void Dispose()
        {
            _logFactory.Dispose();
        }

        // This test will write logs to the console
        [Test]
        public void WireTest()
        {
            var dispatcher = new CommandDispatcher(_logFactory, "testBC");
            var handler = new Handler();
            dispatcher.Wire(handler);
            dispatcher.Dispatch("test", (delay, acknowledge) => { },new Endpoint(), "route");
            dispatcher.Dispatch(1, (delay, acknowledge) => { }, new Endpoint(), "route");
            Assert.That(handler.HandledCommands, Is.EquivalentTo(new object[] { "test", 1 }), "Some commands were not dispatched");
        }
        
        // This test will write logs to the nowhere
        [Test]
        public void WireWithFactoryOptionalParameterNullTest()
        {
            var dispatcher = new CommandDispatcher(EmptyLogFactory.Instance, "testBC");
            var handler = new RepoHandler();
            dispatcher.Wire(handler, new FactoryParameter<IInt64Repo>(() => null));
            dispatcher.Dispatch((Int64)1, (delay, acknowledge) => { }, new Endpoint(), "route");

            Assert.That(handler.HandledCommands, Is.EquivalentTo(new object[] { (Int64)1 }), "Some commands were not dispatched");
        }
    }
```

## In autofac modules

1. You can't obtain ```ILogFactory``` instance at the DI container configuration time, thus it can't be injected to the Autofac module.
2. If some registration depends on log factory instance and needs custom instantiation, use Autofac registration delegate to resolve dependencies only at execution time. Example:

```c#
public class ServiceModule : Module
{
    ...

    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c => Repository.Create(
            c.Resolve<ILogFactory>(),
            _settings.Nested(s => s.Db.ConnString)))
        .As<IRepository>()
        .SingleInstance();
    }
}
```

3. If your existing code logs something right inside Autofac module, consider to move this to the application services (classes). For example 

## Client nugets

Certainly, only client libraries that depends on ```ILog``` should be updated.
To add support of the v5.x to the client library of the some service you should to:

1. Mark methods which accepts ```ILog``` as abslete.
2. Add overloads for these methods with ```ILogFactory``` instead of ```ILog``` or even just without ```ILog```. ```ILog``` should be just removed for ```ServiceCollection``` or ```ContainerBuilder``` extension methods, since ```ILogFactory``` could be obtained inside of these methods.

Example:

```c#
public class RateCalculatorClient : IRateCalculatorClient, IDisposable
{
    private readonly ILog _log;
    private RateCalculatorAPI _service;
 	
 	// This is old constructor
    [Obsolete]
    public RateCalculatorClient(string serviceUrl, ILog log)
    {
        _service = new RateCalculatorAPI(new Uri(serviceUrl), new HttpClient());
        _log = log;
    }
 	
 	// This is new constructor 
    public RateCalculatorClient(string serviceUrl, ILogFactory logFactory)
    {
        _service = new RateCalculatorAPI(new Uri(serviceUrl), new HttpClient());
        _log = logFactory.CreateLog(this);
    }
    
    ...
}

public static class AutofacExtension
{
    // This is old method
    [Obsolete]
    public static void RegisterRateCalculatorClient(
        this ContainerBuilder builder, 
        string serviceUrl, 
        ILog log)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (serviceUrl == null) throw new ArgumentNullException(nameof(serviceUrl));
        if (log == null) throw new ArgumentNullException(nameof(log));
        if (string.IsNullOrWhiteSpace(serviceUrl))
             throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));
  
         builder.RegisterInstance(new RateCalculatorClient(serviceUrl, log)).As<IRateCalculatorClient>().SingleInstance();
    }

    // This is new method. Note that ILog parameter is removed and not replaced with ILogFactory ...
    public static void RegisterRateCalculatorClient(
        this ContainerBuilder builder, 
        string serviceUrl)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (serviceUrl == null) throw new ArgumentNullException(nameof(serviceUrl));
        if (string.IsNullOrWhiteSpace(serviceUrl))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

        // ... instead of this, ILogFactory is resolved inside:
        builder.Register(s => new RateCalculatorClient(serviceUrl, s.Resolve<ILogFactory>()))
            .As<IRateCalculatorClient>()
            .SingleInstance();
    }
}
```

# Injection

1. In versions prior to the v5.x you get used to inject ```ILog``` in the constructors of your classes. In the v5.x you can't inject ```ILog``` and you have to inject ```ILogFactory``` instead.
2. To write something to the log, you still need ```ILog``` instace, to obtain it call ```ILogFactory.CreateLog(this)``` right in the constructor of your class.
3. If you need to distinguish instances of the class in the logs, you could use ```CreateLog``` overload, which accepts ```componentNameSuffix``` parameter.
4. Always made ```ILog``` field in your classes private. Avoid reusing of the another classes log even of base classes.
5. Avoid of ```ILog``` instance registration in the DI container to use it everywhere. This is strongly not recommended.
6. If you need to write something to the ```system-monitoring``` Slack channel, then you need to inject ```IHealthNotifier``` instance to your class.

Example:

```c#
public abstract class TimerPeriod : IStartable, IStopable, ITimerCommand
{
    private ILog _log;
    
    ...
    
    protected TimerPeriod(
        TimeSpan period,
        [NotNull] ILogFactory logFactory,
        [CanBeNull] string componentName = null)
    {
        ...

        _log = componentName == null ? logFactory.CreateLog(this) : logFactory.CreateLog(this, componentName);
    }
}
```

# Logging

1. v5.x has different logging levels. Refer to the [```LogLevel```](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.logLevel?view=aspnetcore-2.1) documentation to learn them.
    1. As you can notice, there is no alternative for ```Monitor``` logging level. It is moved to the sepparate abstraction - ```IHealthNotifier```, so you should use it instead of obsolete ```Monitor``` logging level.
2. There are two overloads of ```ILog``` extension methods for each log level - one overload with implicit ```process``` value (where ```process``` parameter has defalt value of ```null```) and one with explicit ```process``` value (where ```process``` parameter has no default value). If overload with implicit ```process``` value is used, then caller method name will be used as the ```process``` value. These method names are:
    * ```Trace```
    * ```Debug```
    * ```Info```
    * ```Warning```
    * ```Error```
    * ```Critical```
3. In the versions prior to the v5.x, ```message```/```info``` parameter could be null or empty string. In v5.x either ```message``` or ```exception``` should be specified for any log level. Empty or whitespace string message will be treated as absence of the value. Both ```message``` and ```exception``` could be specified for any of the methods.
4. ```context``` could be either string or any object. String ```context``` will be passed as is, whilst any other object will be serialized to the ```Json``` using ```Json.Net``` serializer with following serializer settings:

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

5. Never specify explicit values for the ```callerFilePath``` and ```callerLineNumber``` they marked with ```CallerFilePath``` and ```CallerLineNumber``` attributes respectively and will be filled by the compiler automatically.
6. You don't need to pass ```nameof(MyClass)``` as the ```component``` parameter as you did it for the obsolete ```ILog.WriteXXX``` methods. Name of the class, which does write to the log is obtained when you creates ```ILog``` instance via ```ILogFactory```. Thus you need to remove all of these ```nameof(MyClass)``` from the ```ILog.WriteXXX``` calls.
7. In most cases you don't need to pass ```nameof(MethodName)``` as the ```process``` parameter as you did it for the obsolete ```ILog.WriteXXX``` methods. Name of the method, which does write to the log is obtained automatically, thank's to ```CallerMemberNameAttribute``` (Only, if you using logging method overload, where ```process``` has default value ```null```). Thus you need to remove all these ```nameof(MethodName)``` from the ```ILog.WriteXXX``` calls until you need process name that differs from the containing method name.
8. Methods mapping:
    * abscent in versions prior to v5.x - ```ILog.Trace```, ```ILog.Debug```
    * ```WriteInfo```/```WriteInfoAsync``` - ```ILog.Info```
    * ```WriteWarning```/```WriteWarningAsync``` - ```ILog.Warning```
    * ```WriteError```/```WriteErrorAsync``` - ```ILog.Error```
    * ```WriteFatalError```/```WriteFatalErrorAsync``` - ```ILog.Critical```
    * ```WriteMonitor```/```WriteMonitorAsync``` - ```IHealthNotifier.Notify```

Examples:

```csharp
_log.Trace("Some very detailed message");
_log.Debug("This message could be useful to debug an issue", new { parameterA, PropertyC });
_log.Info("Very useful process", "The process is beign started", processParameters);
_log.Warning("Value can't be parsed, skipping", exception, value);
_log.Error(exception, "Can't proceed processing");
_log.Critical(exception, "Invalid configuration, app will shutdown");
_healthNotifier.Notify("Application has been started");
```