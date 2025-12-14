using MielShop.API.Models;

namespace MielShop.API.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetByIdAsync(int id);
        Task<Cart?> GetByUserIdAsync(int userId); // Changé de string à int
        Task<IEnumerable<Cart>> GetAllAsync();
        Task<Cart> AddAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<CartItem?> GetCartItemAsync(int cartId, int productId);
        Task AddCartItemAsync(CartItem cartItem);
        Task UpdateCartItemAsync(CartItem cartItem);
        Task RemoveCartItemAsync(int cartItemId);
        Task<Cart?> GetUserCartAsync(int userId); // Changé de string à int
    }
}