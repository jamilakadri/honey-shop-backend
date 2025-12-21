using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MielShop.API.Services;
using MielShop.API.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MielShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ICloudinaryService cloudinaryService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // GET: api/products/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveProducts()
        {
            var products = await _productService.GetActiveProductsAsync();
            return Ok(products);
        }

        // GET: api/products/featured
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedProductsAsync()
        {
            var products = await _productService.GetFeaturedProductsAsync();
            return Ok(products);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Produit non trouvé" });
            }
            return Ok(product);
        }

        // GET: api/products/slug/{slug}
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetProductBySlug(string slug)
        {
            var product = await _productService.GetProductBySlugAsync(slug);
            if (product == null)
            {
                return NotFound(new { message = "Produit non trouvé" });
            }
            return Ok(product);
        }

        // GET: api/products/category/5
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            return Ok(products);
        }

        // GET: api/products/search?term=miel
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest(new { message = "Le terme de recherche est requis" });
            }

            var products = await _productService.SearchProductsAsync(term);
            return Ok(products);
        }

        // POST: api/products
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Données invalides",
                    errors = ModelState
                });
            }

            try
            {
                if (product.CategoryId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Veuillez sélectionner une catégorie valide"
                    });
                }

                var createdProduct = await _productService.CreateProductAsync(product);

                return CreatedAtAction(
                    nameof(GetProductById),
                    new { id = createdProduct.ProductId },
                    new
                    {
                        success = true,
                        message = "Produit créé avec succès",
                        data = createdProduct
                    }
                );
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.SqlState == "23503")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La catégorie sélectionnée n'existe pas.",
                        error = "INVALID_CATEGORY"
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "Erreur lors de la création du produit",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Erreur lors de la création du produit",
                    error = ex.Message
                });
            }
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedProduct = await _productService.UpdateProductAsync(id, product);

            if (updatedProduct == null)
            {
                return NotFound(new { message = "Produit non trouvé" });
            }

            return Ok(new { message = "Produit mis à jour avec succès", data = updatedProduct });
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var success = await _productService.DeleteProductAsync(id);

            if (!success)
            {
                return NotFound(new { message = "Produit non trouvé" });
            }

            return Ok(new { message = "Produit supprimé avec succès" });
        }

        // PATCH: api/products/5/stock
        [HttpPatch("{id}/stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int newStock)
        {
            var success = await _productService.UpdateStockAsync(id, newStock);

            if (!success)
            {
                return NotFound(new { message = "Produit non trouvé" });
            }

            return Ok(new { message = "Stock mis à jour avec succès", newStock });
        }

        // ============================================
        // 📸 PRODUCT IMAGES - CLOUDINARY
        // ============================================

        // GET: api/products/{id}/images
        [HttpGet("{id}/images")]
        public async Task<IActionResult> GetProductImages(int id)
        {
            var images = await _productService.GetProductImagesAsync(id);
            return Ok(images);
        }

        // POST: api/products/{id}/images/upload - ✅ CLOUDINARY VERSION
        [HttpPost("{id}/images/upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadProductImage(
            int id,
            [FromForm] IFormFile file,
            [FromForm] string? altText,
            [FromForm] int displayOrder = 0,
            [FromForm] bool isPrimary = false)
        {
            try
            {
                // Verify product exists
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { success = false, message = "Produit non trouvé" });
                }

                // Validate file
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Aucun fichier sélectionné" });
                }

                _logger.LogInformation($"📤 Uploading image to Cloudinary for product {id}");
                _logger.LogInformation($"File: {file.FileName}, Size: {file.Length} bytes");

                // ✅ Upload to Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, "miel-shop/products");

                _logger.LogInformation($"✅ Image uploaded to Cloudinary: {imageUrl}");

                // If isPrimary, unset other primary images
                if (isPrimary)
                {
                    var existingImages = await _productService.GetProductImagesAsync(id);
                    foreach (var existingImage in existingImages.Where(img => img.IsPrimary))
                    {
                        existingImage.IsPrimary = false;
                        await _productService.UpdateProductImageAsync(existingImage);
                    }
                }

                // Create database record with Cloudinary URL
                var productImage = new ProductImage
                {
                    ProductId = id,
                    ImageUrl = imageUrl, // ✅ Cloudinary URL
                    AltText = altText ?? $"{product.Name} - Image",
                    DisplayOrder = displayOrder,
                    IsPrimary = isPrimary,
                    CreatedAt = DateTime.UtcNow
                };

                var createdImage = await _productService.AddProductImageAsync(productImage);

                return Ok(new
                {
                    success = true,
                    message = "Image uploadée avec succès",
                    imageId = createdImage.ImageId,
                    imageUrl = createdImage.ImageUrl,
                    data = createdImage
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"⚠️ Invalid file: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error uploading image: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erreur lors de l'upload", error = ex.Message });
            }
        }

        // POST: api/products/{id}/images - Add image with URL (for existing Cloudinary URLs)
        [HttpPost("{id}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProductImage(int id, [FromBody] ProductImage image)
        {
            image.ProductId = id;
            var createdImage = await _productService.AddProductImageAsync(image);
            return Ok(new { message = "Image ajoutée avec succès", data = createdImage });
        }

        // PUT: api/products/{id}/images/{imageId}
        [HttpPut("{id}/images/{imageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProductImage(int id, int imageId, [FromBody] ProductImage image)
        {
            if (imageId != image.ImageId)
            {
                return BadRequest(new { message = "L'ID d'image ne correspond pas" });
            }

            image.ProductId = id;
            var updatedImage = await _productService.UpdateProductImageAsync(image);

            if (updatedImage == null)
            {
                return NotFound(new { message = "Image non trouvée" });
            }

            return Ok(new { message = "Image mise à jour avec succès", data = updatedImage });
        }

        // DELETE: api/products/{id}/images/{imageId} - ✅ Deletes from Cloudinary too
        [HttpDelete("{id}/images/{imageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProductImage(int id, int imageId)
        {
            var success = await _productService.DeleteProductImageAsync(imageId);

            if (!success)
            {
                return NotFound(new { message = "Image non trouvée" });
            }

            return Ok(new { message = "Image supprimée avec succès" });
        }
    }
}