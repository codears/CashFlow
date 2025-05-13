using CashFlow.Infra.Messaging;
using Microsoft.Extensions.Hosting;

namespace CashierWorker
{
    public class BasicWorker : BackgroundService
    {
        private readonly QueueConsumer _consumer;

        public BasicWorker(QueueConsumer consumer)
        {
            _consumer = consumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.StartConsumingAsync(stoppingToken);
        }
    }
}
