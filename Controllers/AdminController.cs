using Microsoft.AspNetCore.Mvc;
using MielShop.API.Services;

namespace MielShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

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
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ✅ FIXED: Changed route to match frontend call
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
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ✅ FIXED: Changed route to match frontend call
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
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}