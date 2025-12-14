using System.Linq.Expressions;
using MielShop.API.Models;

namespace MielShop.API.Repositories
{
    public interface IWishlistRepository : IRepository<Wishlist>
    {
        Task<IEnumerable<Wishlist>> GetWishlistByUserIdAsync(int userId);
        Task<Wishlist?> GetWishlistItemAsync(int userId, int productId);
        Task<int> GetWishlistCountAsync(int userId);
        Task<bool> AnyAsync(Expression<Func<Wishlist, bool>> predicate);
        Task<IEnumerable<Wishlist>> FindAsync(Expression<Func<Wishlist, bool>> predicate);
        void Delete(Wishlist wishlist);
        void DeleteRange(IEnumerable<Wishlist> wishlists);
    }
}