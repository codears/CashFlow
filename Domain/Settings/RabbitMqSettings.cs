using System.ComponentModel.DataAnnotations;

namespace CashFlow.Domain.Settings
{
    public class RabbitMqSettings
    {
        [Required]
        public string HostName { get; init; }
        [Required]
        public string QueueName { get; init; }
        [Required]
        public string UserName { get; init; }
        [Required]
        public string Password { get; init; }
        [Required]
        public int Port { get; init; } = 5432;
    }
}
