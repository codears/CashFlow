using System.ComponentModel.DataAnnotations;

namespace CashFlow.Domain.Settings
{
    public class RedisSettings
    {
        public string Host { get; init; } = "localhost";
        public int Port { get; init; } = 6379;
        public string? Password { get; init; }
        public int CacheMinutes { get; init; } = 120;
    }
}
