using Microsoft.EntityFrameworkCore;
using MielShop.API.Data;
using MielShop.API.Models;

namespace MielShop.API.Repositories
{
    public class PromoCodeRepository : IPromoCodeRepository
    {
        private readonly ApplicationDbContext _context;

        public PromoCodeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PromoCode?> GetActivePromoByCodeAsync(string code)
        {
            return await _context.PromoCodes
                .Where(p => p.Code == code
                         && p.IsActive
                         && p.StartDate <= DateTime.UtcNow
                         && p.EndDate >= DateTime.UtcNow
                         && p.UsageCount < p.UsageLimit)
                .FirstOrDefaultAsync();
        }
    }
}