using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceSharingApp.Models
{
    public class Revenue
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Foreign Key → Partner who received this revenue
        [Required]
        public int PartnerId { get; set; }

        public Partner? Partner { get; set; }

        // Soft delete column (default true → means active)
        public bool IsDeleted { get; set; } = true;

        //  Navigation property for RevenueShares
        public ICollection<RevenueShare> RevenueShares { get; set; } = new List<RevenueShare>();
    }
}
