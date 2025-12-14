using MielShop.API.Models;
using MielShop.API.Repositories;

namespace MielShop.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _unitOfWork.Categories.GetAllAsync();
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _unitOfWork.Categories.GetActiveCategoriesAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            return await _unitOfWork.Categories.GetByIdAsync(categoryId);
        }

        public async Task<Category?> GetCategoryBySlugAsync(string slug)
        {
            return await _unitOfWork.Categories.GetCategoryBySlugAsync(slug);
        }

        public async Task<Category?> GetCategoryWithProductsAsync(int categoryId)
        {
            return await _unitOfWork.Categories.GetCategoryWithProductsAsync(categoryId);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            category.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            var existingCategory = await _unitOfWork.Categories.GetByIdAsync(category.CategoryId);
            if (existingCategory == null)
                return false;

            existingCategory.Name = category.Name;
            existingCategory.Slug = category.Slug;
            existingCategory.Description = category.Description;
            existingCategory.ImageUrl = category.ImageUrl;
            existingCategory.DisplayOrder = category.DisplayOrder;
            existingCategory.IsActive = category.IsActive;

            _unitOfWork.Categories.Update(existingCategory);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null)
                return false;

            _unitOfWork.Categories.Delete(category);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}