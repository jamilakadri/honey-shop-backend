using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MielShop.API.Services;
using MielShop.API.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using MielShop.API.DTOs.Review ;

namespace MielShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET: api/reviews/product/5
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
            return Ok(reviews);
        }

        // GET: api/reviews/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);

            if (review == null)
            {
                return NotFound(new { message = "Avis non trouvé" });
            }

            return Ok(review);
        }

        // POST: api/reviews
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();

            try
            {
                var review = await _reviewService.CreateReviewAsync(userId, dto);

                return CreatedAtAction(
                    nameof(GetReviewById),
                    new { id = review.ReviewId },
                    review
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/reviews/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();

            try
            {
                var success = await _reviewService.UpdateReviewAsync(id, userId, dto);

                if (!success)
                {
                    return NotFound(new { message = "Avis non trouvé ou non autorisé" });
                }

                return Ok(new { message = "Avis mis à jour" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/reviews/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var success = await _reviewService.DeleteReviewAsync(id, userId, isAdmin);

            if (!success)
            {
                return NotFound(new { message = "Avis non trouvé ou non autorisé" });
            }

            return Ok(new { message = "Avis supprimé" });
        }

        // GET: api/reviews/user/me
        [HttpGet("user/me")]
        [Authorize]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = GetCurrentUserId();
            var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);

            return Ok(reviews);
        }

        // GET: api/reviews/product/5/average
        [HttpGet("product/{productId}/average")]
        public async Task<IActionResult> GetProductAverageRating(int productId)
        {
            var average = await _reviewService.GetProductAverageRatingAsync(productId);
            return Ok(new { averageRating = average });
        }

        // PUT: api/reviews/5/approve (Admin only)
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveReview(int id)
        {
            var success = await _reviewService.ApproveReviewAsync(id);

            if (!success)
            {
                return NotFound(new { message = "Avis non trouvé" });
            }

            return Ok(new { message = "Avis approuvé" });
        }
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingReviews()
        {
            var reviews = await _reviewService.GetPendingReviewsAsync();
            return Ok(reviews);
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