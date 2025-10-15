
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using FinanceSharingApp.Data;
// using FinanceSharingApp.Models;

// namespace FinanceSharingApp.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class ExpensesController : ControllerBase
//     {
//         private readonly AppDbContext _context;

//         public ExpensesController(AppDbContext context)
//         {
//             _context = context;
//         }

//         // ===============================
//         // GET: api/expenses (active only)
//         // ===============================
//         [HttpGet]
//         public async Task<ActionResult<IEnumerable<object>>> GetExpenses()
//         {
//             var expenses = await _context.Expenses
//                 .Include(e => e.Partner)
//                 .Where(e => e.IsDeleted) // true = active
//                 .Select(e => new
//                 {
//                     e.Id,
//                     e.Description,
//                     e.Date,
//                     e.Amount,
//                     PaidBy = e.Partner != null ? e.Partner.Name : "Unknown"
//                 })
//                 .ToListAsync();

//             return Ok(expenses);
//         }

//         // ===============================
//         // GET: api/expenses/{id}
//         // ===============================
//         [HttpGet("{id}")]
//         public async Task<ActionResult<object>> GetExpense(int id)
//         {
//             var expense = await _context.Expenses
//                 .Include(e => e.Partner)
//                 .Where(e => e.Id == id && e.IsDeleted)
//                 .Select(e => new
//                 {
//                     e.Id,
//                     e.Description,
//                     e.Date,
//                     e.Amount,
//                     PaidBy = e.Partner != null ? e.Partner.Name : "Unknown"
//                 })
//                 .FirstOrDefaultAsync();

//             if (expense == null)
//                 return NotFound();

//             return Ok(expense);
//         }

//         // ===============================
//         // POST: api/expenses
//         // ===============================
//         [HttpPost]
//         public async Task<ActionResult<object>> PostExpense([FromBody] Expense expense)
//         {
//             var payer = await _context.Partners.FindAsync(expense.PartnerId);
//             if (payer == null || !payer.IsActive)
//                 return BadRequest(new { message = "Invalid PartnerId or inactive partner." });

//             var activePartners = await _context.Partners
//                 .Where(p => p.IsActive)
//                 .ToListAsync();

//             if (activePartners.Count == 0)
//                 return BadRequest(new { message = "No active partners found." });

//             expense.IsDeleted = true; // active
//             _context.Expenses.Add(expense);
//             await _context.SaveChangesAsync();

//             decimal shareAmount = expense.Amount / activePartners.Count;

//             foreach (var partner in activePartners)
//             {
//                 var share = new ExpenseShare
//                 {
//                     ExpenseId = expense.Id,
//                     PartnerId = partner.Id,
//                     ShareAmount = shareAmount
//                 };
//                 _context.ExpenseShares.Add(share);
//             }

//             await _context.SaveChangesAsync();

//             var response = new
//             {
//                 expense.Id,
//                 expense.Description,
//                 expense.Date,
//                 expense.Amount,
//                 PaidBy = payer.Name,
//                 SharedAmong = activePartners.Select(p => p.Name),
//                 SharePerPerson = shareAmount
//             };

//             return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, response);
//         }

//         // ===============================
//         //  FIXED ENDPOINT
//         // GET: api/expenses/summary
//         // ===============================
//         [HttpGet("summary")]
//         public async Task<ActionResult<IEnumerable<object>>> GetPartnerSummary()
//         {
//             var partners = await _context.Partners
//                 .Where(p => p.IsActive)
//                 .Select(p => new
//                 {
//                     PartnerName = p.Name,
//                     TotalPaid = _context.Expenses
//                         .Where(e => e.PartnerId == p.Id && e.IsDeleted)
//                         .Sum(e => (decimal?)e.Amount) ?? 0,
//                     TotalShare = _context.ExpenseShares
//                         .Where(s => s.PartnerId == p.Id)
//                         .Sum(s => (decimal?)s.ShareAmount) ?? 0
//                 })
//                 .ToListAsync();

//             var result = partners.Select(p => new
//             {
//                 p.PartnerName,
//                 p.TotalPaid,
//                 p.TotalShare,
//                 Balance = p.TotalPaid - p.TotalShare
//             });

//             return Ok(result);
//         }

//         // ===============================
//         // DELETE: api/expenses/{id}
//         // ===============================
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteExpense(int id)
//         {
//             var expense = await _context.Expenses.FindAsync(id);
//             if (expense == null)
//                 return NotFound();

//             expense.IsDeleted = false; // soft delete
//             await _context.SaveChangesAsync();

//             return NoContent();
//         }
//     }
// }
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceSharingApp.Data;
using FinanceSharingApp.Models;

namespace FinanceSharingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        // ===============================
        // GET: api/expenses
        // ===============================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetExpenses()
        {
            var expenses = await _context.Expenses
                .Include(e => e.Partner)
                .Where(e => e.IsDeleted)
                .Select(e => new
                {
                    e.Id,
                    e.Description,
                    e.Date,
                    e.Amount,
                    PaidBy = e.Partner != null ? e.Partner.Name : "Unknown"
                })
                .ToListAsync();

            return Ok(expenses);
        }

        // ===============================
        // GET: api/expenses/{id}
        // ===============================
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetExpense(int id)
        {
            var expense = await _context.Expenses
                .Include(e => e.Partner)
                .Where(e => e.Id == id && e.IsDeleted)
                .Select(e => new
                {
                    e.Id,
                    e.Description,
                    e.Date,
                    e.Amount,
                    PaidBy = e.Partner != null ? e.Partner.Name : "Unknown"
                })
                .FirstOrDefaultAsync();

            if (expense == null)
                return NotFound();

            return Ok(expense);
        }

        // ===============================
        // POST: api/expenses
        // ===============================
        [HttpPost]
        public async Task<ActionResult<object>> PostExpense([FromBody] Expense expense)
        {
            var payer = await _context.Partners.FindAsync(expense.PartnerId);
            if (payer == null || !payer.IsActive)
                return BadRequest(new { message = "Invalid PartnerId or inactive partner." });

            var activePartners = await _context.Partners
                .Where(p => p.IsActive)
                .ToListAsync();

            if (activePartners.Count == 0)
                return BadRequest(new { message = "No active partners found." });

            // ✅ Ensure current UTC date is stored regardless of input
            expense.Date = DateTime.UtcNow;
            expense.IsDeleted = true; // active

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            decimal shareAmount = expense.Amount / activePartners.Count;

            foreach (var partner in activePartners)
            {
                var share = new ExpenseShare
                {
                    ExpenseId = expense.Id,
                    PartnerId = partner.Id,
                    ShareAmount = shareAmount
                };
                _context.ExpenseShares.Add(share);
            }

            await _context.SaveChangesAsync();

            var response = new
            {
                expense.Id,
                expense.Description,
                expense.Date,
                expense.Amount,
                PaidBy = payer.Name,
                SharedAmong = activePartners.Select(p => p.Name),
                SharePerPerson = shareAmount
            };

            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, response);
        }

        // ===============================
        // GET: api/expenses/summary
        // ===============================
        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<object>>> GetPartnerSummary()
        {
            var partners = await _context.Partners
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    PartnerName = p.Name,
                    TotalPaid = _context.Expenses
                        .Where(e => e.PartnerId == p.Id && e.IsDeleted)
                        .Sum(e => (decimal?)e.Amount) ?? 0,
                    TotalShare = _context.ExpenseShares
                        .Where(s => s.PartnerId == p.Id)
                        .Sum(s => (decimal?)s.ShareAmount) ?? 0
                })
                .ToListAsync();

            var result = partners.Select(p => new
            {
                p.PartnerName,
                p.TotalPaid,
                p.TotalShare,
                Balance = p.TotalPaid - p.TotalShare
            });

            return Ok(result);
        }

        // ===============================
        // DELETE: api/expenses/{id}
        // ===============================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            expense.IsDeleted = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
