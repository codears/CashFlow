using CashFlow.Infra.Contexts;
using CashFlow.Infra.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infra.Repositories
{
    public interface IMainRepository
    {
        Task<CashPosting> AddAsync(CashPosting cashPosting);
        Task<List<CashPosting>> GetAllAsync();
    }

    public class MainRepository : IMainRepository
    {
        private readonly CashierDbContext _context;

        public MainRepository(CashierDbContext context)
        {
            _context = context;
        }

        public async Task<CashPosting> AddAsync(CashPosting cashPosting)
        {
            await _context.CashPostings.AddAsync(cashPosting);
            await _context.SaveChangesAsync();
            return cashPosting;
        }

        public async Task<List<CashPosting>> GetAllAsync()
        {
            return await _context.CashPostings
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
