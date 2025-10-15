// using System;
// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace FinanceSharingApp.Models
// {
//     public class Expense
//     {
//         public int Id { get; set; }

//         [Required]
//         [MaxLength(200)]
//         public string Description { get; set; } = null!;

//         [Required]
//         public DateTime Date { get; set; } = DateTime.UtcNow;

//         [Required]
//         [Column(TypeName = "decimal(18,2)")]
//         public decimal Amount { get; set; }

//         // Foreign Key
//         [Required]
//         public int PartnerId { get; set; }

//         public Partner? Partner { get; set; }

//         // Soft delete column (default true → active)
//         public bool IsDeleted { get; set; } = true;

//         //  Navigation property to ExpenseShares
//         public ICollection<ExpenseShare>? ExpenseShares { get; set; }
//     }
// }
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceSharingApp.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = null!;

        // Default date is UTC now, but also enforced in controller
        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public int PartnerId { get; set; }

        public Partner? Partner { get; set; }

        // true = active, false = deleted
        public bool IsDeleted { get; set; } = true;

        public ICollection<ExpenseShare>? ExpenseShares { get; set; }
    }
}
