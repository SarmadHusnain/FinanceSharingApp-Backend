using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceSharingApp.Models
{
    public class RevenueShare
    {
        public int Id { get; set; }

        public int RevenueId { get; set; }
        public Revenue? Revenue { get; set; }

        public int PartnerId { get; set; }
        public Partner? Partner { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShareAmount { get; set; }
    }
}
