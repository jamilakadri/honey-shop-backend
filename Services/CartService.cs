using MielShop.API.Models;
using MielShop.API.Repositories;

namespace MielShop.API.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Cart?> GetCartByUserIdAsync(int userId) // Reste int
        {
            return await _unitOfWork.Carts.GetByUserIdAsync(userId);
        }

        public async Task<CartItem> AddToCartAsync(int userId, int productId, int quantity) // Reste int
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null || !product.IsActive)
            {
                throw new Exception("Produit non disponible");
            }

            if (product.StockQuantity < quantity)
            {
                throw new Exception("Stock insuffisant");
            }

            var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId, // Reste int
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Carts.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.CartItems.Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = product.Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.CartItems.AddAsync(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return existingItem ?? cart.CartItems.Last();
        }

        public async Task<bool> UpdateCartItemAsync(int userId, int cartItemId, int quantity) // Reste int
        {
            var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);
            if (cart == null) return false;

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);
            if (cartItem == null) return false;

            var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
            if (product.StockQuantity < quantity)
            {
                throw new Exception("Stock insuffisant");
            }

            cartItem.Quantity = quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            cart.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.CartItems.Update(cartItem);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(int userId, int cartItemId) // Reste int
        {
            var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);
            if (cart == null) return false;

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);
            if (cartItem == null) return false;

            _unitOfWork.CartItems.Delete(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task ClearCartAsync(int userId) // Reste int
        {
            var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);
            if (cart != null)
            {
                _unitOfWork.CartItems.DeleteRange(cart.CartItems);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<int> GetCartItemCountAsync(int userId) // Reste int
        {
            var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);
            return cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;
        }

        public async Task<decimal> GetCartTotalAsync(int userId) // Reste int
        {
            var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);
            if (cart == null) return 0;

            return cart.CartItems.Sum(ci => ci.Price * ci.Quantity);
        }
    }
}