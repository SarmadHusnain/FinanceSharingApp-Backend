using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceSharingApp.Data;
using FinanceSharingApp.Models;

namespace FinanceSharingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenuesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RevenuesController(AppDbContext context)
        {
            _context = context;
        }

        //  GET: api/revenues (active only)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRevenues()
        {
            var revenues = await _context.Revenues
                .Include(r => r.Partner)
                .Where(r => r.IsDeleted)
                .Select(r => new
                {
                    r.Id,
                    r.Description,
                    r.Date,
                    r.Amount,
                    ReceivedBy = r.Partner != null ? r.Partner.Name : "Unknown"
                })
                .ToListAsync();

            return Ok(revenues);
        }

        //  GET by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetRevenue(int id)
        {
            var revenue = await _context.Revenues
                .Include(r => r.Partner)
                .Where(r => r.Id == id && r.IsDeleted)
                .Select(r => new
                {
                    r.Id,
                    r.Description,
                    r.Date,
                    r.Amount,
                    ReceivedBy = r.Partner != null ? r.Partner.Name : "Unknown"
                })
                .FirstOrDefaultAsync();

            if (revenue == null)
                return NotFound();

            return Ok(revenue);
        }

        //  POST: api/revenues (split shares)
        [HttpPost]
        public async Task<ActionResult<object>> PostRevenue([FromBody] Revenue revenue)
        {
            var partner = await _context.Partners.FindAsync(revenue.PartnerId);
            if (partner == null || !partner.IsActive)
                return BadRequest(new { message = "Invalid PartnerId" });

            revenue.IsDeleted = true;
            _context.Revenues.Add(revenue);
            await _context.SaveChangesAsync();

            // Split equally among all active partners
            var activePartners = await _context.Partners.Where(p => p.IsActive).ToListAsync();
            if (activePartners.Any())
            {
                var shareAmount = revenue.Amount / activePartners.Count;
                foreach (var p in activePartners)
                {
                    _context.RevenueShares.Add(new RevenueShare
                    {
                        RevenueId = revenue.Id,
                        PartnerId = p.Id,
                        ShareAmount = shareAmount
                    });
                }

                await _context.SaveChangesAsync();
            }

            var response = new
            {
                revenue.Id,
                revenue.Description,
                revenue.Date,
                revenue.Amount,
                ReceivedBy = partner.Name
            };

            return CreatedAtAction(nameof(GetRevenue), new { id = revenue.Id }, response);
        }

        //  DELETE (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRevenue(int id)
        {
            var revenue = await _context.Revenues.FindAsync(id);
            if (revenue == null)
                return NotFound();

            revenue.IsDeleted = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //  SUMMARY endpoint
        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<object>>> GetRevenueSummary()
        {
            var activePartners = await _context.Partners
                .Where(p => p.IsActive)
                .ToListAsync();

            var result = new List<object>();

            foreach (var partner in activePartners)
            {
                var totalReceived = await _context.Revenues
                    .Where(r => r.PartnerId == partner.Id && r.IsDeleted)
                    .SumAsync(r => (decimal?)r.Amount) ?? 0;

                var totalShare = await _context.RevenueShares
                    .Where(rs => rs.PartnerId == partner.Id)
                    .SumAsync(rs => (decimal?)rs.ShareAmount) ?? 0;

                result.Add(new
                {
                    PartnerName = partner.Name,
                    TotalReceived = totalReceived,
                    TotalShare = totalShare,
                    Balance = totalReceived - totalShare
                });
            }

            return Ok(result);
        }
    }
}
