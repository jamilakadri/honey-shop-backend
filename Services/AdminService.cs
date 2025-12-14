using MielShop.API.Models;
using MielShop.API.Repositories;
using Microsoft.EntityFrameworkCore;
using MielShop.API.Data; // AJOUTER CE USING

namespace MielShop.API.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public AdminService(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<object> GetDashboardStatsAsync()
        {
            // Utiliser directement le contexte pour les counts
            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders
                .CountAsync(o => o.OrderStatus == "Pending" || o.OrderStatus == "Processing");
            var completedOrders = await _context.Orders
                .CountAsync(o => o.OrderStatus == "Completed");

            var totalRevenue = await _context.Orders
                .Where(o => o.OrderStatus == "Completed")
                .SumAsync(o => o.TotalAmount);

            var totalProducts = await _context.Products.CountAsync();
            var activeProducts = await _context.Products.CountAsync(p => p.IsActive);
            var totalUsers = await _context.Users.CountAsync();
            var pendingReviews = await _context.Reviews.CountAsync(r => !r.IsApproved);

            return new
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                CompletedOrders = completedOrders,
                TotalRevenue = Math.Round(totalRevenue, 2),
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                TotalUsers = totalUsers,
                PendingReviews = pendingReviews
            };
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 5)
        {
            return await _unitOfWork.Orders.GetRecentOrdersAsync(count);
        }

        public async Task<IEnumerable<object>> GetTopSellingProductsAsync(int count = 10)
        {
            var products = await _unitOfWork.Products.GetActiveProductsAsync();

            return products
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Price,
                    p.StockQuantity,
                    CategoryName = p.Category?.Name ?? "Non catégorisé",
                    ImageUrl = p.ProductImages.FirstOrDefault()?.ImageUrl
                })
                .ToList();
        }
    }
}