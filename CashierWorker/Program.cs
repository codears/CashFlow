using CashFlow.Domain.Settings;
using CashierWorker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

    var builder = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        });

    builder.ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMqSettings"));
        services.Configure<RedisSettings>(configuration.GetSection("Redis"));

        var redisSettings = configuration.GetSection("Redis").Get<RedisSettings>();
        if (redisSettings != null)
        {
            var redisConfigString = string.IsNullOrEmpty(redisSettings.Password)
                ? $"{redisSettings.Host}:{redisSettings.Port},abortConnect=false"
                : $"{redisSettings.Host}:{redisSettings.Port},password={redisSettings.Password},abortConnect=false";
            
            Console.WriteLine($"Connecting to Redis at {redisSettings.Host}:{redisSettings.Port}");
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfigString));
        }
        else
        {
            Console.WriteLine("Warning: Redis settings are missing. Worker functionality will be limited.");
        }
        services.AddSingleton<RedisService>();
        services.AddSingleton<QueueConsumer>();
        services.AddHostedService<BasicWorker>();
    });

    await builder.Build().RunAsync();