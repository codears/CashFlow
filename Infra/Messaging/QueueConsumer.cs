using System.Text;
using System.Text.Json;
using CashFlow.Domain.Settings;
using CashFlow.Infra.Entities;
using CashFlow.Infra.Services;
using CashierWorker.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CashFlow.Infra.Messaging
{
    public class QueueConsumer
    {
        private readonly RabbitMqSettings _settings;
        private readonly RedisService _redisService;
        private IModel _channel;
        private IConnection _connection;

        public QueueConsumer(IOptions<RabbitMqSettings> options, RedisService redisService)
        {
            _settings = options.Value;
            _redisService = redisService;

            var factory = new ConnectionFactory()
            {
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            var endpoint = new AmqpTcpEndpoint(_settings.HostName);
            _connection = factory.CreateConnection(new[] { endpoint });
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _settings.QueueName, durable: true, exclusive: false, autoDelete: false);
        }

        public Task StartConsumingAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var cashPosting = JsonSerializer.Deserialize<CashPostingMessage>(message);
                if (cashPosting != null)
                {
                    await UpdateRedis(cashPosting);
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: _settings.QueueName,
                                  autoAck: false,
                                  consumer: consumer);

            return Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    Thread.Sleep(500);
                }

                _channel.Close();
            }, stoppingToken);
        }

        private async Task UpdateRedis(CashPostingMessage message)
        {
            await _redisService.UpdateBalanceAsync(new CashPosting
            {
                Amount = message.Amount,
                PostingType = message.PostingType,
                CreatedAt = message.CreatedAt.DateTime
            });
        }
    }
}
