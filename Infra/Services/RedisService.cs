using System.Globalization;
using CashFlow.Infra.Entities;
using StackExchange.Redis;

namespace CashFlow.Infra.Services
{
    public class RedisService
    {
        private readonly IDatabase _redis;

        public RedisService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task UpdateBalanceAsync(CashPosting cashPosting)
        {
            var key = $"balance:{cashPosting.CreatedAt:yyyy-MM-dd}";
            decimal valor = cashPosting.PostingType == "C" ? cashPosting.Amount : -cashPosting.Amount;
            await _redis.StringIncrementAsync(key, Convert.ToDouble(valor, CultureInfo.InvariantCulture));
        }
    }
}