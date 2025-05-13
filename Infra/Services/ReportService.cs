using System.Globalization;
using CashFlow.Infra.Contexts;
using CashReportApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace CashFlow.Infra.Services
{
    public interface IReportService
    {
        Task<Response<BalanceResponse>> GetBalanceByDateAsync(DateOnly date);
    }

    public class ReportService : IReportService
    {
        private readonly CashierDbContext _context;
        private readonly IDatabase _redis;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            CashierDbContext context,
            IConnectionMultiplexer redis,
            ILogger<ReportService> logger)
        {
            _context = context;
            _redis = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<Response<BalanceResponse>> GetBalanceByDateAsync(DateOnly date)
        {
            try
            {
                string key = $"balance:{date:yyyy-MM-dd}";

                var redisValue = await _redis.StringGetAsync(key);
                if (redisValue.HasValue && decimal.TryParse(redisValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var balance))
                {
                    return new BalanceResponse { Amount = Math.Round(balance, 2), Date = date };
                }

                var cashPostings = await _context.CashPostings
                    .Where(l => DateOnly.FromDateTime(l.CreatedAt) == date)
                    .ToListAsync();

                decimal calculatedBalance = cashPostings.Sum(x =>
                    x.PostingType == "C" ? x.Amount : -x.Amount
                );

                await _redis.StringSetAsync(key, calculatedBalance.ToString(CultureInfo.InvariantCulture), TimeSpan.FromHours(1));

                return new BalanceResponse { Amount = Math.Round(calculatedBalance, 2), Date = date };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Error.ContactAdministrator;
            }
        }
    }
}
