using MielShop.API.Models;

namespace MielShop.API.Repositories
{
    public interface ICartItemRepository : IRepository<CartItem>
    {
        Task<CartItem?> GetCartItemAsync(int cartId, int productId);
        Task<IEnumerable<CartItem>> GetCartItemsByCartIdAsync(int cartId);
    }
}