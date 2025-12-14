using MielShop.API.Models;
using MielShop.API.DTOs.Review;
using MielShop.API.Repositories;

namespace MielShop.API.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId)
        {
            return await _unitOfWork.Reviews.GetApprovedReviewsByProductIdAsync(productId);
        }

        public async Task<Review?> GetReviewByIdAsync(int reviewId)
        {
            return await _unitOfWork.Reviews.GetReviewWithDetailsAsync(reviewId);
        }

        public async Task<Review> CreateReviewAsync(int userId, CreateReviewDto dto)
        {
            // Vérifier si l'utilisateur a déjà laissé un avis pour ce produit
            var existingReview = await _unitOfWork.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == dto.ProductId);

            if (existingReview != null)
            {
                throw new Exception("Vous avez déjà laissé un avis pour ce produit");
            }

            var review = new Review
            {
                UserId = userId,
                ProductId = dto.ProductId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            return review;
        }

        public async Task<bool> UpdateReviewAsync(int reviewId, int userId, UpdateReviewDto dto)
        {
            var review = await _unitOfWork.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.UserId == userId);

            if (review == null) return false;

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.IsApproved = false;

            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId, bool isAdmin)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null) return false;

            if (review.UserId != userId && !isAdmin)
            {
                return false;
            }

            _unitOfWork.Reviews.Delete(review);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId)
        {
            return await _unitOfWork.Reviews.GetReviewsByUserIdAsync(userId);
        }

        public async Task<double> GetProductAverageRatingAsync(int productId)
        {
            return await _unitOfWork.Reviews.GetProductAverageRatingAsync(productId);
        }

        public async Task<bool> ApproveReviewAsync(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null) return false;

            review.IsApproved = true;
            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Review>> GetPendingReviewsAsync()
        {
            return await _unitOfWork.Reviews.GetPendingReviewsAsync();
        }
    }
}
