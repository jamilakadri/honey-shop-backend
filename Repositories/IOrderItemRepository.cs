using MielShop.API.Models;
namespace MielShop.API.Repositories
{
    public interface IOrderItemRepository
    {
        Task AddAsync(OrderItem orderItem);
        // Ajoutez d'autres méthodes selon vos besoins
    }
}