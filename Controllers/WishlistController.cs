using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MielShop.API.Services;
using System.Security.Claims;

namespace MielShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        // GET: api/wishlist
        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = GetCurrentUserId();
            var wishlistItems = await _wishlistService.GetWishlistByUserIdAsync(userId);

            return Ok(wishlistItems);
        }

        // POST: api/wishlist/5
        [HttpPost("{productId}")]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = GetCurrentUserId();

            try
            {
                var wishlistItem = await _wishlistService.AddToWishlistAsync(userId, productId);
                return Ok(new { message = "Produit ajouté aux favoris", data = wishlistItem });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/wishlist/5
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userId = GetCurrentUserId();
            var success = await _wishlistService.RemoveFromWishlistAsync(userId, productId);

            if (!success)
            {
                return NotFound(new { message = "Produit non trouvé dans les favoris" });
            }

            return Ok(new { message = "Produit retiré des favoris" });
        }

        // GET: api/wishlist/check/5
        [HttpGet("check/{productId}")]
        public async Task<IActionResult> IsInWishlist(int productId)
        {
            var userId = GetCurrentUserId();
            var isInWishlist = await _wishlistService.IsInWishlistAsync(userId, productId);

            return Ok(new { isInWishlist });
        }

        // DELETE: api/wishlist
        [HttpDelete]
        public async Task<IActionResult> ClearWishlist()
        {
            var userId = GetCurrentUserId();
            await _wishlistService.ClearWishlistAsync(userId);

            return Ok(new { message = "Liste de souhaits vidée" });
        }

        // GET: api/wishlist/count
        [HttpGet("count")]
        public async Task<IActionResult> GetWishlistCount()
        {
            var userId = GetCurrentUserId();
            var count = await _wishlistService.GetWishlistCountAsync(userId);

            return Ok(new { count });
        }

        // POST: api/wishlist/move-to-cart/5
        [HttpPost("move-to-cart/{productId}")]
        public async Task<IActionResult> MoveToCart(int productId)
        {
            var userId = GetCurrentUserId();

            try
            {
                await _wishlistService.MoveToCartAsync(userId, productId);
                return Ok(new { message = "Produit déplacé vers le panier" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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