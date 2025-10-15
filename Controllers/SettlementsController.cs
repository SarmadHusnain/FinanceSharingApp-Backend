// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using FinanceSharingApp.Data;
// using FinanceSharingApp.Models;

// namespace FinanceSharingApp.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class SettlementsController : ControllerBase
//     {
//         private readonly AppDbContext _context;

//         public SettlementsController(AppDbContext context)
//         {
//             _context = context;
//         }

//         // ===============================================
//         // GET: api/settlements?from=2025-01-01&to=2025-12-31
//         // ===============================================
//         [HttpGet]
//         public async Task<ActionResult<object>> GetSettlement(
//             [FromQuery] DateTime? from,
//             [FromQuery] DateTime? to)
//         {
//             DateTime fromDate = from ?? DateTime.MinValue;
//             DateTime toDate = to ?? DateTime.MaxValue;

//             // Step 1: Get all active partners
//             var partners = await _context.Partners
//                 .Where(p => p.IsActive)
//                 .ToListAsync();

//             if (!partners.Any())
//                 return BadRequest(new { message = "No active partners found." });

//             // Step 2: Get revenues and expenses within the date range
//             var revenues = await _context.Revenues
//                 .Where(r => r.IsDeleted && r.Date >= fromDate && r.Date <= toDate)
//                 .ToListAsync();

//             var expenses = await _context.Expenses
//                 .Where(e => e.IsDeleted && e.Date >= fromDate && e.Date <= toDate)
//                 .ToListAsync();

//             // Step 3: Calculate total per partner
//             var partnerData = partners.Select(p => new
//             {
//                 Partner = p,
//                 TotalRevenue = revenues.Where(r => r.PartnerId == p.Id).Sum(r => r.Amount),
//                 TotalExpense = expenses.Where(e => e.PartnerId == p.Id).Sum(e => e.Amount)
//             }).ToList();

//             // Step 4: Compute averages
//             decimal avgRevenue = partnerData.Average(p => p.TotalRevenue);
//             decimal avgExpense = partnerData.Average(p => p.TotalExpense);

//             // Step 5: Calculate each partner’s net balance
//             var balances = partnerData.Select(p => new PartnerBalance
//             {
//                 Id = p.Partner.Id,
//                 Name = p.Partner.Name,
//                 TotalRevenue = p.TotalRevenue,
//                 TotalExpense = p.TotalExpense,
//                 NetBalance = Math.Round((p.TotalRevenue - avgRevenue) - (p.TotalExpense - avgExpense), 2)
//             }).ToList();

//             // Step 6: Separate creditors and debtors (mutable)
//             var debtors = balances
//                 .Where(b => b.NetBalance < 0)
//                 .Select(b => new Debtor { Name = b.Name, Owes = Math.Abs(b.NetBalance) })
//                 .ToList();

//             var creditors = balances
//                 .Where(b => b.NetBalance > 0)
//                 .Select(b => new Creditor { Name = b.Name, ToReceive = b.NetBalance })
//                 .ToList();

//             // Step 7: Compute settlements
//             var settlements = new List<object>();

//             foreach (var debtor in debtors)
//             {
//                 decimal oweAmount = debtor.Owes;

//                 foreach (var creditor in creditors.Where(c => c.ToReceive > 0))
//                 {
//                     if (oweAmount <= 0)
//                         break;

//                     decimal payment = Math.Min(oweAmount, creditor.ToReceive);
//                     oweAmount -= payment;
//                     creditor.ToReceive -= payment;

//                     settlements.Add(new
//                     {
//                         From = debtor.Name,
//                         To = creditor.Name,
//                         Amount = Math.Round(payment, 2)
//                     });
//                 }
//             }

//             // Step 8: Prepare final response
//             var result = new
//             {
//                 DateRange = new
//                 {
//                     From = fromDate.ToString("yyyy-MM-dd"),
//                     To = toDate.ToString("yyyy-MM-dd")
//                 },
//                 PartnerBalances = balances,
//                 Settlements = settlements
//             };

//             return Ok(result);
//         }

//         // Helper classes for mutability
//         private class Creditor
//         {
//             public string Name { get; set; } = string.Empty;
//             public decimal ToReceive { get; set; }
//         }

//         private class Debtor
//         {
//             public string Name { get; set; } = string.Empty;
//             public decimal Owes { get; set; }
//         }

//         private class PartnerBalance
//         {
//             public int Id { get; set; }
//             public string Name { get; set; } = string.Empty;
//             public decimal TotalRevenue { get; set; }
//             public decimal TotalExpense { get; set; }
//             public decimal NetBalance { get; set; }
//         }
//     }
// }
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceSharingApp.Data;
using FinanceSharingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceSharingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettlementsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SettlementsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/settlements?from=2025-01-01&to=2025-12-31
        [HttpGet]
        public async Task<ActionResult<object>> GetSettlement(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            try
            {
                DateTime fromDate = from ?? DateTime.MinValue;
                DateTime toDate = to ?? DateTime.MaxValue;

                // Validate date range
                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be later than to date." });

                // Step 1: Get all partners and admins
                var partners = await _context.Partners
                    .Where(p => p.Role == "Partner" || p.Role == "Admin")
                    .ToListAsync();

                if (!partners.Any())
                    return BadRequest(new { message = "No partners or admins found." });

                // Step 1.5: Compute the last date in the database
                DateTime maxRevDate = DateTime.MinValue;
                var activeRevQuery = _context.Revenues.Where(r => r.IsDeleted); // IsDeleted = true for active
                if (await activeRevQuery.AnyAsync())
                {
                    maxRevDate = await activeRevQuery.MaxAsync(r => r.Date);
                }

                DateTime maxExpDate = DateTime.MinValue;
                var activeExpQuery = _context.Expenses.Where(e => e.IsDeleted); // IsDeleted = true for active
                if (await activeExpQuery.AnyAsync())
                {
                    maxExpDate = await activeExpQuery.MaxAsync(e => e.Date);
                }

                DateTime maxDbDate = maxRevDate > maxExpDate ? maxRevDate : maxExpDate;

                // Step 2: Get active revenues and expenses within the date range
                var revenues = await _context.Revenues
                    .Where(r => r.IsDeleted && r.Date >= fromDate && r.Date <= toDate)
                    .ToListAsync();

                var expenses = await _context.Expenses
                    .Where(e => e.IsDeleted && e.Date >= fromDate && e.Date <= toDate)
                    .ToListAsync();

                // Step 3: Calculate total per partner within the date range
                var partnerData = new List<PartnerData>();
                foreach (var p in partners)
                {
                    DateTime pStart = fromDate;
                    DateTime pEnd = toDate;

                    if (p.IsActive)
                    {
                        pEnd = toDate < maxDbDate ? toDate : maxDbDate;
                    }
                    else if (p.RemoveDate.HasValue)
                    {
                        pEnd = toDate < p.RemoveDate.Value ? toDate : p.RemoveDate.Value;
                    }

                    if (pEnd < pStart)
                        continue;

                    decimal totalRevenue = revenues
                        .Where(r => r.PartnerId == p.Id && r.Date >= pStart && r.Date <= pEnd)
                        .Sum(r => r.Amount);
                    decimal totalExpense = expenses
                        .Where(e => e.PartnerId == p.Id && e.Date >= pStart && e.Date <= pEnd)
                        .Sum(e => e.Amount);

                    partnerData.Add(new PartnerData
                    {
                        Partner = p,
                        TotalRevenue = totalRevenue,
                        TotalExpense = totalExpense
                    });
                }

                // Include all partners in balances, even those with no transactions
                var balances = partners.Select(p =>
                {
                    var data = partnerData.FirstOrDefault(pd => pd.Partner.Id == p.Id);
                    return new PartnerBalance
                    {
                        Id = p.Id,
                        Name = p.Name,
                        TotalRevenue = data?.TotalRevenue ?? 0m,
                        TotalExpense = data?.TotalExpense ?? 0m,
                        NetBalance = 0m,
                        IsActive = p.IsActive
                    };
                }).ToList();

                // Step 4: Compute averages
                decimal avgRevenue = partnerData.Any() ? partnerData.Average(p => p.TotalRevenue) : 0m;
                decimal avgExpense = partnerData.Any() ? partnerData.Average(p => p.TotalExpense) : 0m;

                // Step 5: Calculate each partner’s net balance
                foreach (var balance in balances)
                {
                    var data = partnerData.FirstOrDefault(pd => pd.Partner.Id == balance.Id);
                    if (data != null)
                    {
                        balance.NetBalance = Math.Round((data.TotalRevenue - avgRevenue) - (data.TotalExpense - avgExpense), 2);
                    }
                }

                // Step 6: Separate creditors and debtors
                var debtors = balances
                    .Where(b => b.NetBalance < 0)
                    .Select(b => new Debtor { Name = b.Name, Owes = Math.Abs(b.NetBalance) })
                    .ToList();

                var creditors = balances
                    .Where(b => b.NetBalance > 0)
                    .Select(b => new Creditor { Name = b.Name, ToReceive = b.NetBalance })
                    .ToList();

                // Step 7: Compute settlements
                var settlements = new List<object>();
                foreach (var debtor in debtors)
                {
                    decimal oweAmount = debtor.Owes;
                    foreach (var creditor in creditors.Where(c => c.ToReceive > 0))
                    {
                        if (oweAmount <= 0)
                            break;

                        decimal payment = Math.Min(oweAmount, creditor.ToReceive);
                        oweAmount -= payment;
                        creditor.ToReceive -= payment;

                        settlements.Add(new
                        {
                            From = debtor.Name,
                            To = creditor.Name,
                            Amount = Math.Round(payment, 2)
                        });
                    }
                }

                // Step 8: Prepare final response
                var result = new
                {
                    DateRange = new
                    {
                        From = fromDate.ToString("yyyy-MM-dd"),
                        To = toDate.ToString("yyyy-MM-dd")
                    },
                    PartnerBalances = balances,
                    Settlements = settlements
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSettlement: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { message = "An error occurred while calculating settlements.", details = ex.Message });
            }
        }

        // NEW ENDPOINT: GET api/settlements/{id}?from=2025-01-01&to=2025-12-31
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetSettlementByUserId(
            int id,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            try
            {
                DateTime fromDate = from ?? DateTime.MinValue;
                DateTime toDate = to ?? DateTime.MaxValue;

                // Validate date range
                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be later than to date." });

                // Step 1: Get the specific partner/admin
                var partner = await _context.Partners
                    .Where(p => p.Id == id && (p.Role == "Partner" || p.Role == "Admin"))
                    .FirstOrDefaultAsync();

                if (partner == null)
                    return NotFound(new { message = "Partner or admin not found." });

                // Step 1.5: Get all partners for average calculation
                var allPartners = await _context.Partners
                    .Where(p => p.Role == "Partner" || p.Role == "Admin")
                    .ToListAsync();

                if (!allPartners.Any())
                    return BadRequest(new { message = "No partners or admins found for average calculation." });

                // Step 2: Compute the last date in the database
                DateTime maxRevDate = DateTime.MinValue;
                var activeRevQuery = _context.Revenues.Where(r => r.IsDeleted);
                if (await activeRevQuery.AnyAsync())
                {
                    maxRevDate = await activeRevQuery.MaxAsync(r => r.Date);
                }

                DateTime maxExpDate = DateTime.MinValue;
                var activeExpQuery = _context.Expenses.Where(e => e.IsDeleted);
                if (await activeExpQuery.AnyAsync())
                {
                    maxExpDate = await activeExpQuery.MaxAsync(e => e.Date);
                }

                DateTime maxDbDate = maxRevDate > maxExpDate ? maxRevDate : maxExpDate;

                // Step 3: Get active revenues and expenses within the date range
                var revenues = await _context.Revenues
                    .Where(r => r.IsDeleted && r.Date >= fromDate && r.Date <= toDate)
                    .ToListAsync();

                var expenses = await _context.Expenses
                    .Where(e => e.IsDeleted && e.Date >= fromDate && e.Date <= toDate)
                    .ToListAsync();

                // Step 4: Calculate total per partner within the date range
                var partnerData = new List<PartnerData>();
                foreach (var p in allPartners)
                {
                    DateTime pStart = fromDate;
                    DateTime pEnd = toDate;

                    if (p.IsActive)
                    {
                        pEnd = toDate < maxDbDate ? toDate : maxDbDate;
                    }
                    else if (p.RemoveDate.HasValue)
                    {
                        pEnd = toDate < p.RemoveDate.Value ? toDate : p.RemoveDate.Value;
                    }

                    if (pEnd < pStart)
                        continue;

                    decimal totalRevenue = revenues
                        .Where(r => r.PartnerId == p.Id && r.Date >= pStart && r.Date <= pEnd)
                        .Sum(r => r.Amount);
                    decimal totalExpense = expenses
                        .Where(e => e.PartnerId == p.Id && e.Date >= pStart && e.Date <= pEnd)
                        .Sum(e => e.Amount);

                    partnerData.Add(new PartnerData
                    {
                        Partner = p,
                        TotalRevenue = totalRevenue,
                        TotalExpense = totalExpense
                    });
                }

                // Step 5: Compute averages
                decimal avgRevenue = partnerData.Any() ? partnerData.Average(p => p.TotalRevenue) : 0m;
                decimal avgExpense = partnerData.Any() ? partnerData.Average(p => p.TotalExpense) : 0m;

                // Step 6: Calculate balance for the specific partner
                var partnerBalance = new PartnerBalance();
                var specificPartnerData = partnerData.FirstOrDefault(pd => pd.Partner.Id == id);
                if (specificPartnerData != null)
                {
                    partnerBalance = new PartnerBalance
                    {
                        Id = partner.Id,
                        Name = partner.Name,
                        TotalRevenue = specificPartnerData.TotalRevenue,
                        TotalExpense = specificPartnerData.TotalExpense,
                        NetBalance = Math.Round((specificPartnerData.TotalRevenue - avgRevenue) - (specificPartnerData.TotalExpense - avgExpense), 2),
                        IsActive = partner.IsActive
                    };
                }
                else
                {
                    partnerBalance = new PartnerBalance
                    {
                        Id = partner.Id,
                        Name = partner.Name,
                        TotalRevenue = 0m,
                        TotalExpense = 0m,
                        NetBalance = 0m,
                        IsActive = partner.IsActive
                    };
                }

                // Step 7: Compute settlements for the specific partner
                var settlements = new List<object>();
                if (partnerBalance.NetBalance < 0)
                {
                    // Partner is a debtor
                    decimal oweAmount = Math.Abs(partnerBalance.NetBalance);
                    var creditors = partnerData
                        .Where(pd => pd.TotalRevenue - avgRevenue - (pd.TotalExpense - avgExpense) > 0)
                        .Select(pd => new Creditor
                        {
                            Name = pd.Partner.Name,
                            ToReceive = Math.Round((pd.TotalRevenue - avgRevenue) - (pd.TotalExpense - avgExpense), 2)
                        })
                        .ToList();

                    foreach (var creditor in creditors.Where(c => c.ToReceive > 0))
                    {
                        if (oweAmount <= 0)
                            break;

                        decimal payment = Math.Min(oweAmount, creditor.ToReceive);
                        oweAmount -= payment;
                        creditor.ToReceive -= payment;

                        settlements.Add(new
                        {
                            From = partnerBalance.Name,
                            To = creditor.Name,
                            Amount = Math.Round(payment, 2)
                        });
                    }
                }
                else if (partnerBalance.NetBalance > 0)
                {
                    // Partner is a creditor
                    decimal toReceive = partnerBalance.NetBalance;
                    var debtors = partnerData
                        .Where(pd => pd.TotalRevenue - avgRevenue - (pd.TotalExpense - avgExpense) < 0)
                        .Select(pd => new Debtor
                        {
                            Name = pd.Partner.Name,
                            Owes = Math.Abs(Math.Round((pd.TotalRevenue - avgRevenue) - (pd.TotalExpense - avgExpense), 2))
                        })
                        .ToList();

                    foreach (var debtor in debtors.Where(d => d.Owes > 0))
                    {
                        if (toReceive <= 0)
                            break;

                        decimal payment = Math.Min(toReceive, debtor.Owes);
                        toReceive -= payment;
                        debtor.Owes -= payment;

                        settlements.Add(new
                        {
                            From = debtor.Name,
                            To = partnerBalance.Name,
                            Amount = Math.Round(payment, 2)
                        });
                    }
                }

                // Step 8: Prepare final response
                var result = new
                {
                    DateRange = new
                    {
                        From = fromDate.ToString("yyyy-MM-dd"),
                        To = toDate.ToString("yyyy-MM-dd")
                    },
                    PartnerBalance = partnerBalance,
                    Settlements = settlements
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSettlementByUserId: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { message = "An error occurred while calculating user settlement.", details = ex.Message });
            }
        }

        // Helper classes
        private class Creditor
        {
            public string Name { get; set; } = string.Empty;
            public decimal ToReceive { get; set; }
        }

        private class Debtor
        {
            public string Name { get; set; } = string.Empty;
            public decimal Owes { get; set; }
        }

        private class PartnerBalance
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal TotalRevenue { get; set; }
            public decimal TotalExpense { get; set; }
            public decimal NetBalance { get; set; }
            public bool IsActive { get; set; }
        }

        private class PartnerData
        {
            public Partner Partner { get; set; } = null!;
            public decimal TotalRevenue { get; set; }
            public decimal TotalExpense { get; set; }
        }
    }
}