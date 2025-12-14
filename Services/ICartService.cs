using MielShop.API.Models;
using MielShop.API.Repositories;

namespace MielShop.API.Services
{
    public interface ICartService
    {
        Task<Cart?> GetCartByUserIdAsync(int userId);
        Task<CartItem> AddToCartAsync(int userId, int productId, int quantity);
        Task<bool> UpdateCartItemAsync(int userId, int cartItemId, int quantity);
        Task<bool> RemoveFromCartAsync(int userId, int cartItemId);
        Task ClearCartAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);
        Task<decimal> GetCartTotalAsync(int userId);
    }
}