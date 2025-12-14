using MielShop.API.Models;

namespace MielShop.API.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetFeaturedProductsAsync();
        Task<Product?> GetProductByIdAsync(int productId);
        Task<Product?> GetProductWithDetailsAsync(int productId);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<Product> CreateProductAsync(Product product);
        Task<Product?> UpdateProductAsync(int productId, Product product);
        Task<bool> DeleteProductAsync(int productId);
        Task<bool> UpdateStockAsync(int productId, int newStock);

        // CHANGE THESE:
        Task<Product?> GetProductBySlugAsync(string slug);  // Was: Task<Product>
        Task<IEnumerable<ProductImage>> GetProductImagesAsync(int productId);  // Was: Task<List<ProductImage>>

        Task<ProductImage> AddProductImageAsync(ProductImage image);
        Task<ProductImage?> UpdateProductImageAsync(ProductImage image);
        Task<bool> DeleteProductImageAsync(int imageId);
    }
}