using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MielShop.API.Services;
using MielShop.API.Models;

namespace MielShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
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
                return BadRequest(ModelState);
            }

            var createdProduct = await _productService.CreateProductAsync(product);

            return CreatedAtAction(
                nameof(GetProductById),
                new { id = createdProduct.ProductId },
                new { message = "Produit créé avec succès", data = createdProduct }
            );
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
        // Add to ProductsController.cs - AFTER existing endpoints
        // In ProductsController.cs, add ALL these image endpoints:

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

        // GET: api/products/{id}/images
        [HttpGet("{id}/images")]
        public async Task<IActionResult> GetProductImages(int id)
        {
            var images = await _productService.GetProductImagesAsync(id);
            return Ok(images);
        }

        // POST: api/products/{id}/images
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

        // DELETE: api/products/{id}/images/{imageId}
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
        // POST: api/products/{id}/images/upload
        // POST: api/products/{id}/images/upload
        [HttpPost("{id}/images/upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadProductImage(
            int id,
            [FromForm] IFormFile file,
            [FromForm] string? altText,
            [FromForm] int displayOrder = 0,
            [FromForm] bool isPrimary = false)
        {
            // 1. Vérifier que le produit existe
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Produit non trouvé" });
            }

            // 2. Valider le fichier
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Aucun fichier sélectionné" });
            }

            // 3. Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Format d'image non supporté. Utilisez JPG, PNG, GIF ou WebP" });
            }

            // 4. Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "L'image est trop volumineuse (max 5MB)" });
            }

            try
            {
                // 5. Create uploads directory if it doesn't exist
                // ✅ CORRIGÉ: Même structure que les catégories (sans wwwroot)
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "products");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 6. Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 7. Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 8. Si isPrimary, désactiver les autres images primaires
                if (isPrimary)
                {
                    var existingImages = await _productService.GetProductImagesAsync(id);
                    foreach (var existingImage in existingImages.Where(img => img.IsPrimary))
                    {
                        existingImage.IsPrimary = false;
                        await _productService.UpdateProductImageAsync(existingImage);
                    }
                }

                // 9. Create database record
                var imageUrl = $"/uploads/products/{uniqueFileName}";
                var productImage = new ProductImage
                {
                    ProductId = id,
                    ImageUrl = imageUrl,
                    AltText = altText ?? file.FileName,
                    DisplayOrder = displayOrder,
                    IsPrimary = isPrimary,
                    CreatedAt = DateTime.UtcNow
                };

                var createdImage = await _productService.AddProductImageAsync(productImage);

                return Ok(new
                {
                    message = "Image uploadée avec succès",
                    imageId = createdImage.ImageId,
                    imageUrl = createdImage.ImageUrl,
                    data = createdImage
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur upload: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Erreur lors de l'upload", error = ex.Message });
            }
        }
    }
}