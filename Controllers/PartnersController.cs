// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using FinanceSharingApp.Data;
// using FinanceSharingApp.Models;

// namespace FinanceSharingApp.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class PartnersController : ControllerBase
//     {
//         private readonly AppDbContext _context;

//         public PartnersController(AppDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/partners → sirf active partners
//         [HttpGet]
//         public async Task<ActionResult<IEnumerable<Partner>>> GetPartners()
//         {
//             return await _context.Partners
//                 .Where(p => p.IsActive)
//                 .ToListAsync();
//         }

//         // GET: api/partners/{id}
//         [HttpGet("{id}")]
//         public async Task<ActionResult<Partner>> GetPartner(int id)
//         {
//             var partner = await _context.Partners.FindAsync(id);
//             if (partner == null)
//                 return NotFound();

//             return partner;
//         }

//         // POST: api/partners
//         [HttpPost]
//         public async Task<ActionResult<Partner>> PostPartner(Partner partner)
//         {
//             bool emailExists = await _context.Partners
//                 .AnyAsync(p => p.Email == partner.Email && p.IsActive);

//             if (emailExists)
//             {
//                 return BadRequest(new { message = "Email already exists!" });
//             }

//             partner.IsActive = true;
//             partner.RemoveDate = null;

//             _context.Partners.Add(partner);
//             await _context.SaveChangesAsync();

//             return CreatedAtAction(nameof(GetPartner), new { id = partner.Id }, partner);
//         }

//         // DELETE (Soft Delete): api/partners/{id}
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeletePartner(int id)
//         {
//             var partner = await _context.Partners.FindAsync(id);
//             if (partner == null)
//                 return NotFound();

//             partner.IsActive = false;
//             partner.RemoveDate = DateTime.UtcNow; // store deletion time

//             await _context.SaveChangesAsync();

//             return NoContent();
//         }
//     }
// }
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceSharingApp.Data;
using FinanceSharingApp.Models;
using BCrypt.Net; 

namespace FinanceSharingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartnersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PartnersController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================
        // GET: api/partners → active only
        // ==========================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPartners()
        {
            var partners = await _context.Partners
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Email,
                    p.Role,
                    p.DateJoined,
                    p.IsActive
                })
                .ToListAsync();

            return Ok(partners);
        }

        // ==========================
        // GET: api/partners/{id}
        // ==========================
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPartner(int id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null)
                return NotFound();

            return new
            {
                partner.Id,
                partner.Name,
                partner.Email,
                partner.Role,
                partner.DateJoined,
                partner.IsActive
            };
        }

        // ==========================
        // POST: api/partners (create new)
        // ==========================
        [HttpPost]
        public async Task<ActionResult<object>> PostPartner([FromBody] Partner partner)
        {
            bool emailExists = await _context.Partners
                .AnyAsync(p => p.Email == partner.Email && p.IsActive);

            if (emailExists)
                return BadRequest(new { message = "Email already exists!" });

            //  Ensure Role value
            if (string.IsNullOrWhiteSpace(partner.Role))
                partner.Role = "Partner";

            //  Password Hashing (important)
            if (string.IsNullOrWhiteSpace(partner.PasswordHash))
                return BadRequest(new { message = "Password is required." });

            partner.PasswordHash = BCrypt.Net.BCrypt.HashPassword(partner.PasswordHash);

            partner.IsActive = true;
            partner.RemoveDate = null;
            partner.DateJoined = DateTime.UtcNow;

            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();

            //  Hide sensitive info in response
            var response = new
            {
                partner.Id,
                partner.Name,
                partner.Email,
                partner.Role,
                partner.DateJoined,
                partner.IsActive
            };

            return CreatedAtAction(nameof(GetPartner), new { id = partner.Id }, response);
        }

        // ==========================
        // DELETE (soft delete)
        // ==========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePartner(int id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null)
                return NotFound();

            partner.IsActive = false;
            partner.RemoveDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
