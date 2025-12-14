using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MielShop.API.Data;

namespace MielShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("database")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var categoriesCount = await _context.Categories.CountAsync();
                return Ok(new
                {
                    message = "Connexion reussie !",
                    categoriesCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erreur de connexion",
                    error = ex.Message

                });
            }
        }
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }
    }
}