using Microsoft.EntityFrameworkCore;
using MielShop.API.Data;
using MielShop.API.Models;

namespace MielShop.API.Repositories
{
	public class CartItemRepository : Repository<CartItem>, ICartItemRepository
	{
		public CartItemRepository(ApplicationDbContext context) : base(context)
		{
		}

		public async Task<CartItem?> GetCartItemAsync(int cartId, int productId)
		{
			return await _dbSet
				.FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
		}
        public async Task<IEnumerable<CartItem>> GetCartItemsByCartIdAsync(int cartId)
        {
            return await _dbSet
                .Where(ci => ci.CartId == cartId)
                .Include(ci => ci.Product)
                .ToListAsync();
        }
    }
}