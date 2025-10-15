using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceSharingApp.Models
{
    public class ExpenseShare
    {
        public int Id { get; set; }

        [Required]
        public int ExpenseId { get; set; }

        [ForeignKey(nameof(ExpenseId))]
        public Expense Expense { get; set; } = null!;

        [Required]
        public int PartnerId { get; set; }

        [ForeignKey(nameof(PartnerId))]
        public Partner Partner { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShareAmount { get; set; }
    }
}
