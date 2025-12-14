using MielShop.API.Models;
namespace MielShop.API.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int userId);
        // Ajoutez d'autres méthodes selon vos besoins
    }
}