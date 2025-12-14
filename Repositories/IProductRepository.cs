using MielShop.API.Models;

namespace MielShop.API.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetFeaturedProductsAsync();
        Task<Product?> GetProductWithDetailsAsync(int productId);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<Product?> GetProductBySlugAsync(string slug);
        Task<IEnumerable<ProductImage>> GetProductImagesAsync(int productId);
        Task<bool> DeleteProductImageAsync(int imageId);
    }
}