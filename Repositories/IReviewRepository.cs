using MielShop.API.Models;

namespace MielShop.API.Repositories
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetApprovedReviewsByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId);
        Task<IEnumerable<Review>> GetPendingReviewsAsync();
        Task<Review?> GetReviewWithDetailsAsync(int reviewId);
        Task<double> GetProductAverageRatingAsync(int productId);
    }
}