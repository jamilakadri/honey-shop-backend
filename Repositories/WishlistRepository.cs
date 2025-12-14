using Microsoft.EntityFrameworkCore;
using MielShop.API.Data;
using MielShop.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MielShop.API.Repositories
{
    public class WishlistRepository : Repository<Wishlist>, IWishlistRepository
    {
        public WishlistRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Wishlist>> GetWishlistByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(w => w.UserId == userId)
                .Include(w => w.Product)
                .ToListAsync();
        }

        public async Task<Wishlist?> GetWishlistItemAsync(int userId, int productId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        public async Task<int> GetWishlistCountAsync(int userId)
        {
            return await _dbSet
                .CountAsync(w => w.UserId == userId);
        }

        public async Task<bool> AnyAsync(Expression<Func<Wishlist, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<IEnumerable<Wishlist>> FindAsync(Expression<Func<Wishlist, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public void Delete(Wishlist wishlist)
        {
            _dbSet.Remove(wishlist);
        }

        public void DeleteRange(IEnumerable<Wishlist> wishlists)
        {
            _dbSet.RemoveRange(wishlists);
        }
    }
}