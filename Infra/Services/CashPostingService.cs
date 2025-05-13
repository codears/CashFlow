using CashFlow.Domain.Models;
using CashFlow.Infra.Entities;
using CashFlow.Infra.Repositories;
using CashFlow.Infra.Messaging;

namespace CashFlow.Infra.Services
{
    public interface ICashPostingService
    {
        Task<CashPosting> CreateCashPostingAsync(CashPostingRequest request);
        Task<List<CashPosting>> GetAllAsync();
    }

    public class CashPostingService : ICashPostingService
    {
        private readonly IMainRepository _repository;
        private readonly QueueProducer _queueProducer;

        public CashPostingService(IMainRepository repository, QueueProducer queueProducer)
        {
            _repository = repository;
            _queueProducer = queueProducer;
        }

        public async Task<CashPosting> CreateCashPostingAsync(CashPostingRequest request)
        {
            var cashPosting = new CashPosting
            {
                Amount = request.Amount,
                PostingType = request.PostingType,
                Description = request.Description
            };

            var createdCashPosting = await _repository.AddAsync(cashPosting);

            await _queueProducer.PublishAsync(createdCashPosting);

            return createdCashPosting;
        }

        public async Task<List<CashPosting>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
