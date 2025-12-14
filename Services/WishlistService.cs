using MielShop.API.Models;
using MielShop.API.Repositories;

namespace MielShop.API.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WishlistService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Wishlist>> GetWishlistByUserIdAsync(int userId)
        {
            return await _unitOfWork.Wishlists.GetWishlistByUserIdAsync(userId);
        }

        public async Task<Wishlist> AddToWishlistAsync(int userId, int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                throw new Exception("Produit non trouvé");
            }

            var existing = await _unitOfWork.Wishlists.GetWishlistItemAsync(userId, productId);
            if (existing != null)
            {
                throw new Exception("Ce produit est déjà dans vos favoris");
            }

            var wishlist = new Wishlist
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Wishlists.AddAsync(wishlist);
            await _unitOfWork.SaveChangesAsync();
            return wishlist;
        }

        public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
        {
            var wishlist = await _unitOfWork.Wishlists.GetWishlistItemAsync(userId, productId);
            if (wishlist == null) return false;

            _unitOfWork.Wishlists.Delete(wishlist);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsInWishlistAsync(int userId, int productId)
        {
            return await _unitOfWork.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        public async Task ClearWishlistAsync(int userId)
        {
            var items = await _unitOfWork.Wishlists.FindAsync(w => w.UserId == userId);
            _unitOfWork.Wishlists.DeleteRange(items);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetWishlistCountAsync(int userId)
        {
            return await _unitOfWork.Wishlists.GetWishlistCountAsync(userId);
        }

        public async Task MoveToCartAsync(int userId, int productId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var wishlist = await _unitOfWork.Wishlists.GetWishlistItemAsync(userId, productId);
                if (wishlist == null)
                    throw new Exception("Produit non trouvé dans les favoris");

                var cart = await _unitOfWork.Carts.GetUserCartAsync(userId);
                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Carts.AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync();
                }

                var existingCartItem = await _unitOfWork.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == productId);
                if (existingCartItem != null)
                {
                    existingCartItem.Quantity++;
                    existingCartItem.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.CartItems.Update(existingCartItem);
                }
                else
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(productId);
                    var cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = productId,
                        Quantity = 1,
                        Price = product!.Price,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.CartItems.AddAsync(cartItem);
                }

                _unitOfWork.Wishlists.Delete(wishlist);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}