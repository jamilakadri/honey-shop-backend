using MielShop.API.Models;

namespace MielShop.API.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<Category?> GetCategoryBySlugAsync(string slug);
        Task<Category?> GetCategoryWithProductsAsync(int categoryId);
    }
}