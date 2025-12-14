using MielShop.API.Models;

namespace MielShop.API.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int categoryId);
        Task<Category?> GetCategoryBySlugAsync(string slug);
        Task<Category?> GetCategoryWithProductsAsync(int categoryId);
        Task<Category> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Category category); // Changé pour retourner bool et prendre un seul paramètre
        Task<bool> DeleteCategoryAsync(int categoryId);
    }
}