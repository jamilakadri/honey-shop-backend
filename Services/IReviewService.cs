using MielShop.API.Models;
using MielShop.API.DTOs.Review;

namespace MielShop.API.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
        Task<Review?> GetReviewByIdAsync(int reviewId);
        Task<Review> CreateReviewAsync(int userId, CreateReviewDto dto);
        Task<bool> UpdateReviewAsync(int reviewId, int userId, UpdateReviewDto dto);
        Task<bool> DeleteReviewAsync(int reviewId, int userId, bool isAdmin);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId);
        Task<double> GetProductAverageRatingAsync(int productId);
        Task<bool> ApproveReviewAsync(int reviewId);
        Task<IEnumerable<Review>> GetPendingReviewsAsync();
    }
}