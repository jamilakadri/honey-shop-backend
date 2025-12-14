using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MielShop.API.Data;
using MielShop.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MielShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication
    public class AddressesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AddressesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Addresses - Get all addresses for current user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetMyAddresses()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId.Value)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(addresses);
        }

        // GET: api/Addresses/5 - Get specific address by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Address>> GetAddress(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId.Value);

            if (address == null)
                return NotFound();

            return Ok(address);
        }

        // POST: api/Addresses - Create new address
        [HttpPost]
        public async Task<ActionResult<Address>> CreateAddress([FromBody] CreateAddressDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            // If this is set as default, unset other defaults of the same type
            if (dto.IsDefault)
            {
                var existingDefaults = await _context.Addresses
                    .Where(a => a.UserId == userId.Value &&
                               a.AddressType == dto.AddressType &&
                               a.IsDefault)
                    .ToListAsync();

                foreach (var existing in existingDefaults)
                {
                    existing.IsDefault = false;
                }
            }

            var address = new Address
            {
                UserId = userId.Value,
                AddressType = dto.AddressType,
                FullName = dto.FullName,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country ?? "Tunisia",
                PhoneNumber = dto.PhoneNumber,
                IsDefault = dto.IsDefault,
                CreatedAt = DateTime.UtcNow
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAddress), new { id = address.AddressId }, address);
        }

        // PUT: api/Addresses/5 - Update address
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateAddressDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId.Value);

            if (address == null)
                return NotFound();

            // If setting as default, unset other defaults
            if (dto.IsDefault == true && !address.IsDefault)
            {
                var existingDefaults = await _context.Addresses
                    .Where(a => a.UserId == userId.Value &&
                               a.AddressType == address.AddressType &&
                               a.IsDefault &&
                               a.AddressId != id)
                    .ToListAsync();

                foreach (var existing in existingDefaults)
                {
                    existing.IsDefault = false;
                }
            }

            // Update fields
            if (dto.AddressType != null) address.AddressType = dto.AddressType;
            if (dto.FullName != null) address.FullName = dto.FullName;
            if (dto.AddressLine1 != null) address.AddressLine1 = dto.AddressLine1;
            if (dto.AddressLine2 != null) address.AddressLine2 = dto.AddressLine2;
            if (dto.City != null) address.City = dto.City;
            if (dto.State != null) address.State = dto.State;
            if (dto.PostalCode != null) address.PostalCode = dto.PostalCode;
            if (dto.Country != null) address.Country = dto.Country;
            if (dto.PhoneNumber != null) address.PhoneNumber = dto.PhoneNumber;
            if (dto.IsDefault.HasValue) address.IsDefault = dto.IsDefault.Value;

            await _context.SaveChangesAsync();

            return Ok(address);
        }

        // DELETE: api/Addresses/5 - Delete address
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId.Value);

            if (address == null)
                return NotFound();

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Addresses/5/set-default - Set address as default
        [HttpPut("{id}/set-default")]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == userId.Value);

            if (address == null)
                return NotFound();

            // Unset other defaults of the same type
            var existingDefaults = await _context.Addresses
                .Where(a => a.UserId == userId.Value &&
                           a.AddressType == address.AddressType &&
                           a.IsDefault &&
                           a.AddressId != id)
                .ToListAsync();

            foreach (var existing in existingDefaults)
            {
                existing.IsDefault = false;
            }

            address.IsDefault = true;
            await _context.SaveChangesAsync();

            return Ok(address);
        }

        // GET: api/Addresses/default/shipping - Get default shipping address
        [HttpGet("default/shipping")]
        public async Task<ActionResult<Address>> GetDefaultShippingAddress()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == userId.Value &&
                                        a.AddressType == "Shipping" &&
                                        a.IsDefault);

            if (address == null)
                return NotFound(new { message = "No default shipping address found" });

            return Ok(address);
        }

        // GET: api/Addresses/default/billing - Get default billing address
        [HttpGet("default/billing")]
        public async Task<ActionResult<Address>> GetDefaultBillingAddress()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == userId.Value &&
                                        a.AddressType == "Billing" &&
                                        a.IsDefault);

            if (address == null)
                return NotFound(new { message = "No default billing address found" });

            return Ok(address);
        }

        // Helper method to get current user ID from JWT token
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }

    // DTOs
    public class CreateAddressDto
    {
        public string AddressType { get; set; } = "Shipping"; // Shipping, Billing, Both
        public string FullName { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string? Country { get; set; } = "Tunisia";
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
    }

    public class UpdateAddressDto
    {
        public string? AddressType { get; set; }
        public string? FullName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsDefault { get; set; }
    }
}