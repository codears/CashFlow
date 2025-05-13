using CashFlow.Domain.Settings;
using CashFlow.Infra.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace CashFlow.CashierApi.Tests.IntegrationTests
{
    public class HealthCheckTests
    {
        private readonly IConfiguration _configuration;

        public HealthCheckTests()
        {
            // Load configuration from appsettings.test.json
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.test.json")
                .Build();
        }

        [Fact]
        public void Redis_Connection_IsHealthy()
        {
            // Arrange
            var redisSettings = _configuration.GetSection("Redis").Get<RedisSettings>();

            // Act
            var connectionString = string.IsNullOrEmpty(redisSettings.Password)
                ? $"{redisSettings.Host}:{redisSettings.Port},abortConnect=false"
                : $"{redisSettings.Host}:{redisSettings.Port},password={redisSettings.Password},abortConnect=false";

            var connection = ConnectionMultiplexer.Connect(connectionString);
            var database = connection.GetDatabase();

            // Assert
            Assert.True(connection.IsConnected, "Redis connection should be established");

            // Additional check - try a ping operation
            var ping = database.Ping();
            Assert.True(ping.TotalMilliseconds > 0, "Redis ping should return a valid response time");

            // Clean up
            connection.Dispose();
        }

        [Fact]
        public void RabbitMQ_Connection_IsHealthy()
        {
            // Arrange
            var rabbitMqSettings = _configuration.GetSection("RabbitMqSettings").Get<RabbitMqSettings>();

            // Act & Assert
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = rabbitMqSettings.HostName,
                    UserName = rabbitMqSettings.UserName,
                    Password = rabbitMqSettings.Password,
                    Port = rabbitMqSettings.Port,
                    RequestedHeartbeat = TimeSpan.FromSeconds(60)
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                // Attempt to declare the queue as a passive operation (checks if it exists)
                channel.QueueDeclarePassive(rabbitMqSettings.QueueName);

                Assert.True(connection.IsOpen, "RabbitMQ connection should be open");
            }
            catch (Exception ex)
            {
                Assert.Fail($"RabbitMQ connection failed: {ex.Message}");
            }
        }

        [Fact]
        public async Task PostgreSQL_Connection_IsHealthy()
        {
            // Arrange
            var connectionString = _configuration.GetConnectionString("PostgreSql");

            var options = new DbContextOptionsBuilder<CashierDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            // Act & Assert
            try
            {
                using var context = new CashierDbContext(options);
                var canConnect = await context.Database.CanConnectAsync();

                Assert.True(canConnect, "PostgreSQL database connection should be successful");
            }
            catch (Exception ex)
            {
                Assert.Fail($"PostgreSQL connection failed: {ex.Message}");
            }
        }
    }
}
