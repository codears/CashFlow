using System.ComponentModel.DataAnnotations;

namespace CashFlow.Domain.Settings
{
    public class RedisSettings
    {
        [Required]
        public string Host { get; init; }
        [Required]
        public int Port { get; init; }
        public string? Password { get; init; }
        public int CacheMinutes { get; init; } = 120;
    }
}
