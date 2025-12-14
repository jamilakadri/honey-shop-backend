using MielShop.API.Models;
namespace MielShop.API.Repositories
{
    public interface IPromoCodeRepository
    {
        Task<PromoCode?> GetActivePromoByCodeAsync(string code);
        // Ajoutez d'autres méthodes selon vos besoins
    }
}