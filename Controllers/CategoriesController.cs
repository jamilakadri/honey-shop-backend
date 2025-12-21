using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MielShop.API.Services;
using MielShop.API.Models;

namespace MielShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ICategoryService categoryService,
            ICloudinaryService cloudinaryService,
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/categories/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCategories()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/categories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound(new { message = "Catégorie non trouvée" });
            }

            return Ok(category);
        }

        // GET: api/categories/slug/miel-fleurs
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            var category = await _categoryService.GetCategoryBySlugAsync(slug);

            if (category == null)
            {
                return NotFound(new { message = "Catégorie non trouvée" });
            }

            return Ok(category);
        }

        // GET: api/categories/5/products
        [HttpGet("{id}/products")]
        public async Task<IActionResult> GetCategoryWithProducts(int id)
        {
            var category = await _categoryService.GetCategoryWithProductsAsync(id);

            if (category == null)
            {
                return NotFound(new { message = "Catégorie non trouvée" });
            }

            return Ok(category);
        }

        // POST: api/categories
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdCategory = await _categoryService.CreateCategoryAsync(category);

            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = createdCategory.CategoryId },
                createdCategory
            );
        }

        // PUT: api/categories/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            if (id != category.CategoryId)
            {
                return BadRequest(new { message = "L'ID ne correspond pas" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _categoryService.UpdateCategoryAsync(category);

            if (!success)
            {
                return NotFound(new { message = "Catégorie non trouvée" });
            }

            return NoContent();
        }

        // DELETE: api/categories/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);

            if (!success)
            {
                return NotFound(new { message = "Catégorie non trouvée" });
            }

            return NoContent();
        }

        // ============================================
        // 📸 CATEGORY IMAGES - CLOUDINARY
        // ============================================

        // POST: api/categories/upload-image - ✅ CLOUDINARY VERSION
        [HttpPost("upload-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadCategoryImage([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Aucun fichier fourni" });
                }

                _logger.LogInformation($"📤 Uploading category image to Cloudinary");
                _logger.LogInformation($"File: {file.FileName}, Size: {file.Length} bytes");

                // ✅ Upload to Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, "miel-shop/categories");

                _logger.LogInformation($"✅ Category image uploaded to Cloudinary: {imageUrl}");

                return Ok(new
                {
                    success = true,
                    message = "Image uploadée avec succès",
                    imageUrl = imageUrl,
                    fileName = Path.GetFileName(file.FileName)
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"⚠️ Invalid file: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error uploading category image: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erreur lors de l'upload de l'image", error = ex.Message });
            }
        }

        // POST: api/categories/{categoryId}/image - Upload and assign to category
        [HttpPost("{categoryId}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadAndAssignCategoryImage(int categoryId, [FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Aucun fichier fourni" });
                }

                // Get existing category
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    return NotFound(new { success = false, message = $"Catégorie avec ID {categoryId} non trouvée" });
                }

                _logger.LogInformation($"📤 Uploading and assigning image to category {categoryId}");

                // Delete old image from Cloudinary if exists
                if (!string.IsNullOrEmpty(category.ImageUrl))
                {
                    _logger.LogInformation($"🗑️ Deleting old category image from Cloudinary");
                    await _cloudinaryService.DeleteImageAsync(category.ImageUrl);
                }

                // Upload new image to Cloudinary
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, "miel-shop/categories");

                _logger.LogInformation($"✅ Image uploaded to Cloudinary: {imageUrl}");

                // Update category with new image URL
                category.ImageUrl = imageUrl;
                //category.UpdatedAt = DateTime.UtcNow;

                var updated = await _categoryService.UpdateCategoryAsync(category);

                if (!updated)
                {
                    return StatusCode(500, new { success = false, message = "Échec de la mise à jour de la catégorie" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Image de catégorie uploadée et assignée avec succès",
                    data = new { imageUrl = imageUrl, category = category }
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"⚠️ Invalid file: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error uploading category image: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erreur lors de l'upload de l'image", error = ex.Message });
            }
        }

        // DELETE: api/categories/delete-image?imageUrl=... - ✅ CLOUDINARY VERSION
        [HttpDelete("delete-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategoryImage([FromQuery] string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return BadRequest(new { success = false, message = "URL de l'image requise" });
            }

            try
            {
                _logger.LogInformation($"🗑️ Deleting category image from Cloudinary: {imageUrl}");

                // ✅ Delete from Cloudinary
                var deleted = await _cloudinaryService.DeleteImageAsync(imageUrl);

                if (deleted)
                {
                    return Ok(new { success = true, message = "Image supprimée avec succès de Cloudinary" });
                }
                else
                {
                    return NotFound(new { success = false, message = "Image non trouvée sur Cloudinary" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting category image: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erreur lors de la suppression de l'image", error = ex.Message });
            }
        }

        // DELETE: api/categories/{categoryId}/image - Delete category's assigned image
        [HttpDelete("{categoryId}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategoryAssignedImage(int categoryId)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    return NotFound(new { success = false, message = "Catégorie non trouvée" });
                }

                if (string.IsNullOrEmpty(category.ImageUrl))
                {
                    return BadRequest(new { success = false, message = "La catégorie n'a pas d'image" });
                }

                _logger.LogInformation($"🗑️ Deleting image for category {categoryId}");

                // Delete from Cloudinary
                await _cloudinaryService.DeleteImageAsync(category.ImageUrl);

                // Update category
                category.ImageUrl = null;
                //category.UpdatedAt = DateTime.UtcNow;
                await _categoryService.UpdateCategoryAsync(category);

                return Ok(new { success = true, message = "Image de catégorie supprimée avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting category image: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Erreur lors de la suppression", error = ex.Message });
            }
        }
    }
}