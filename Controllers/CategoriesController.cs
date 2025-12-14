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
        private readonly IWebHostEnvironment _environment;

        public CategoriesController(ICategoryService categoryService, IWebHostEnvironment environment)
        {
            _categoryService = categoryService;
            _environment = environment;
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

        // ✅ NEW: POST: api/categories/upload-image
        [HttpPost("upload-image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadCategoryImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Aucun fichier fourni" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Format de fichier non autorisé. Utilisez JPG, PNG, GIF ou WebP" });
            }

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "Le fichier est trop volumineux. Taille maximale: 5MB" });
            }

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "categories");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return the relative URL
                var imageUrl = $"/uploads/categories/{fileName}";

                return Ok(new
                {
                    message = "Image uploadée avec succès",
                    imageUrl = imageUrl,
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de l'upload de l'image", error = ex.Message });
            }
        }

        // ✅ NEW: DELETE: api/categories/delete-image?imageUrl=/uploads/categories/xxx.jpg
        [HttpDelete("delete-image")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteCategoryImage([FromQuery] string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return BadRequest(new { message = "URL de l'image requise" });
            }

            try
            {
                // Extract filename from URL
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "categories", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Ok(new { message = "Image supprimée avec succès" });
                }
                else
                {
                    return NotFound(new { message = "Image non trouvée" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de la suppression de l'image", error = ex.Message });
            }
        }
    }
}