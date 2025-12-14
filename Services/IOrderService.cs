using MielShop.API.Models;
using MielShop.API.DTOs.Order;

namespace MielShop.API.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId); // Changé de string à int
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<Order> CreateOrderAsync(int userId, CreateOrderDto dto); // Changé de string à int
        Task<bool> CancelOrderAsync(int orderId, int userId); // Changé de string à int
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetPendingOrdersAsync();
        Task<object> GetOrderStatisticsAsync();
    }
}