using System.Text;
using System.Text.Json;
using CashFlow.Domain.Settings;
using CashFlow.Infra.Entities;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CashFlow.Infra.Messaging
{
    public class QueueProducer
    {
        private readonly RabbitMqSettings _settings;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public QueueProducer(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;

            var factory = new ConnectionFactory()
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _settings.QueueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }

        public async Task PublishAsync(CashPosting cashPosting)
        {
            ArgumentNullException.ThrowIfNull(cashPosting);
            var message = JsonSerializer.Serialize(cashPosting);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                                  routingKey: _settings.QueueName,
                                  basicProperties: null,
                                  body: body);

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
