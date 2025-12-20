using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MielShop.API.Services;
using MielShop.API.Models;

namespace MielShop.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAdminService adminService,
            IProductService productService,
            ICategoryService categoryService,
            ICloudinaryService cloudinaryService,
            ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _productService = productService;
            _categoryService = categoryService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        // ============================================
        // 📊 DASHBOARD STATS
        // ============================================

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await _adminService.GetDashboardStatsAsync();
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting dashboard stats: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("orders/recent")]
        public async Task<IActionResult> GetRecentOrders()
        {
            try
            {
                var orders = await _adminService.GetRecentOrdersAsync(5);
                return Ok(new { success = true, data = orders });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting recent orders: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("products/top")]
        public async Task<IActionResult> GetTopProducts()
        {
            try
            {
                var products = await _adminService.GetTopSellingProductsAsync(10);
                return Ok(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting top products: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // 📦 PRODUCT IMAGE UPLOAD - CLOUDINARY
        // ============================================

        [HttpPost("products/{productId}/images")]
        public async Task<IActionResult> UploadProductImage(
            int productId,
            [FromForm] IFormFile file,
            [FromForm] int displayOrder = 0,
            [FromForm] bool isPrimary = false)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No file uploaded" });
                }

                _logger.LogInformation($"📤 Uploading product image for product {productId}");
                _logger.LogInformation($"File: {file.FileName}, Size: {file.Length} bytes, ContentType: {file.ContentType}");

                // Get product to check if it exists
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(new { success = false, message = $"Product with ID {productId} not found" });
                }

                // Upload to Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, "miel-shop/products");

                // Create product image record
                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = imageUrl,
                    AltText = $"{product.Name} - Image {displayOrder}",
                    DisplayOrder = displayOrder,
                    IsPrimary = isPrimary,
                    CreatedAt = DateTime.UtcNow
                };

                var createdImage = await _productService.AddProductImageAsync(productImage);

                _logger.LogInformation($"✅ Product image uploaded successfully: {imageUrl}");

                return Ok(new
                {
                    success = true,
                    message = "Image uploaded successfully",
                    data = createdImage
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"⚠️ Invalid file upload: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error uploading product image: {ex.Message}");
                return StatusCode(500, new { success = false, message = $"Error uploading image: {ex.Message}" });
            }
        }

        [HttpGet("products/{productId}/images")]
        public async Task<IActionResult> GetProductImages(int productId)
        {
            try
            {
                var images = await _productService.GetProductImagesAsync(productId);
                return Ok(new { success = true, data = images });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting product images: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("products/{productId}/images/{imageId}")]
        public async Task<IActionResult> DeleteProductImage(int productId, int imageId)
        {
            try
            {
                _logger.LogInformation($"🗑️ Deleting product image {imageId} for product {productId}");

                var success = await _productService.DeleteProductImageAsync(imageId);

                if (!success)
                {
                    return NotFound(new { success = false, message = "Image not found" });
                }

                _logger.LogInformation($"✅ Product image deleted successfully");

                return Ok(new { success = true, message = "Image deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting product image: {ex.Message}");
                return StatusCode(500, new { success = false, message = $"Error deleting image: {ex.Message}" });
            }
        }

        [HttpPut("products/{productId}/images/{imageId}")]
        public async Task<IActionResult> UpdateProductImage(
            int productId,
            int imageId,
            [FromBody] ProductImage imageData)
        {
            try
            {
                imageData.ImageId = imageId;
                imageData.ProductId = productId;

                var updatedImage = await _productService.UpdateProductImageAsync(imageData);

                if (updatedImage == null)
                {
                    return NotFound(new { success = false, message = "Image not found" });
                }

                return Ok(new { success = true, message = "Image updated successfully", data = updatedImage });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating product image: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // 🗂️ CATEGORY IMAGE UPLOAD - CLOUDINARY
        // ============================================

        [HttpPost("categories/{categoryId}/image")]
        public async Task<IActionResult> UploadCategoryImage(int categoryId, [FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No file uploaded" });
                }

                _logger.LogInformation($"📤 Uploading category image for category {categoryId}");
                _logger.LogInformation($"File: {file.FileName}, Size: {file.Length} bytes, ContentType: {file.ContentType}");

                // Get existing category
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    return NotFound(new { success = false, message = $"Category with ID {categoryId} not found" });
                }

                // Delete old image from Cloudinary if exists
                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    _logger.LogInformation($"🗑️ Deleting old category image: {category.ImageUrl}");
                    await _cloudinaryService.DeleteImageAsync(category.ImageUrl);
                }

                // Upload new image to Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, "miel-shop/categories");

                // Update category with new image URL
                category.ImageUrl = imageUrl;
                //category.UpdatedAt = DateTime.UtcNow;

                var updated = await _categoryService.UpdateCategoryAsync(category);

                if (!updated)
                {
                    return StatusCode(500, new { success = false, message = "Failed to update category" });
                }

                _logger.LogInformation($"✅ Category image uploaded successfully: {imageUrl}");

                return Ok(new
                {
                    success = true,
                    message = "Category image uploaded successfully",
                    data = new { imageUrl = imageUrl, category = category }
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"⚠️ Invalid file upload: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error uploading category image: {ex.Message}");
                return StatusCode(500, new { success = false, message = $"Error uploading image: {ex.Message}" });
            }
        }

        [HttpDelete("categories/{categoryId}/image")]
        public async Task<IActionResult> DeleteCategoryImage(int categoryId)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    return NotFound(new { success = false, message = "Category not found" });
                }

                if (string.IsNullOrEmpty(category.ImageUrl))
                {
                    return BadRequest(new { success = false, message = "Category has no image" });
                }

                // Delete from Cloudinary
                await _cloudinaryService.DeleteImageAsync(category.ImageUrl);

                // Update category
                category.ImageUrl = null;
                //category.UpdatedAt = DateTime.UtcNow;
                await _categoryService.UpdateCategoryAsync(category);

                return Ok(new { success = true, message = "Category image deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting category image: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // 📋 BULK UPLOAD (Optional - for multiple images at once)
        // ============================================

        [HttpPost("products/{productId}/images/bulk")]
        public async Task<IActionResult> BulkUploadProductImages(int productId, [FromForm] List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest(new { success = false, message = "No files uploaded" });
                }

                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(new { success = false, message = $"Product with ID {productId} not found" });
                }

                _logger.LogInformation($"📤 Bulk uploading {files.Count} images for product {productId}");

                var uploadedImages = new List<ProductImage>();
                var errors = new List<string>();

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    try
                    {
                        var imageUrl = await _cloudinaryService.UploadImageAsync(file, "miel-shop/products");

                        var productImage = new ProductImage
                        {
                            ProductId = productId,
                            ImageUrl = imageUrl,
                            AltText = $"{product.Name} - Image {i + 1}",
                            DisplayOrder = i,
                            IsPrimary = i == 0 && uploadedImages.Count == 0,
                            CreatedAt = DateTime.UtcNow
                        };

                        var createdImage = await _productService.AddProductImageAsync(productImage);
                        uploadedImages.Add(createdImage);

                        _logger.LogInformation($"✅ Uploaded image {i + 1}/{files.Count}: {file.FileName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"❌ Error uploading image {file.FileName}: {ex.Message}");
                        errors.Add($"{file.FileName}: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = $"Uploaded {uploadedImages.Count} of {files.Count} images",
                    data = uploadedImages,
                    errors = errors.Count > 0 ? errors : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in bulk upload: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}