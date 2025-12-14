using MielShop.API.Models;

namespace MielShop.API.Services
{
    public interface IWishlistService
    {
        Task<IEnumerable<Wishlist>> GetWishlistByUserIdAsync(int userId);
        Task<Wishlist> AddToWishlistAsync(int userId, int productId);
        Task<bool> RemoveFromWishlistAsync(int userId, int productId);
        Task<bool> IsInWishlistAsync(int userId, int productId);
        Task ClearWishlistAsync(int userId);
        Task<int> GetWishlistCountAsync(int userId);
        Task MoveToCartAsync(int userId, int productId);
    }
}