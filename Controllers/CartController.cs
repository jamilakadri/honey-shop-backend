using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MielShop.API.Services;
using MielShop.API.Models;
using System.Security.Claims;
using MielShop.API.DTOs.Cart ; 

namespace MielShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET: api/cart
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetCurrentUserId();
            var cart = await _cartService.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                return Ok(new { items = new List<object>(), totalAmount = 0 });
            }

            return Ok(cart);
        }

        // POST: api/cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();

            try
            {
                var cartItem = await _cartService.AddToCartAsync(userId, dto.ProductId, dto.Quantity);
                return Ok(new { message = "Produit ajouté au panier", data = cartItem });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/cart/items/5
        [HttpPut("items/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();

            try
            {
                var success = await _cartService.UpdateCartItemAsync(userId, cartItemId, dto.Quantity);

                if (!success)
                {
                    return NotFound(new { message = "Article non trouvé dans le panier" });
                }

                return Ok(new { message = "Quantité mise à jour" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/cart/items/5
        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = GetCurrentUserId();
            var success = await _cartService.RemoveFromCartAsync(userId, cartItemId);

            if (!success)
            {
                return NotFound(new { message = "Article non trouvé dans le panier" });
            }

            return Ok(new { message = "Article retiré du panier" });
        }

        // DELETE: api/cart
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            await _cartService.ClearCartAsync(userId);

            return Ok(new { message = "Panier vidé" });
        }

        // GET: api/cart/count
        [HttpGet("count")]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userId = GetCurrentUserId();
            var count = await _cartService.GetCartItemCountAsync(userId);

            return Ok(new { count });
        }

        // GET: api/cart/total
        [HttpGet("total")]
        public async Task<IActionResult> GetCartTotal()
        {
            var userId = GetCurrentUserId();
            var total = await _cartService.GetCartTotalAsync(userId);

            return Ok(new { total });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Utilisateur non authentifié");
            }
            return int.Parse(userIdClaim.Value);
        }
    }

   
}