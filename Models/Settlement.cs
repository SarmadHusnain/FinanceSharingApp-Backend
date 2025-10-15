// using System;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace FinanceSharingApp.Models
// {
//     public class Settlement
//     {
//         public int Id { get; set; }

//         // Debtor (who will pay)
//         public int FromPartnerId { get; set; }
//         public Partner? FromPartner { get; set; }

//         // Creditor (who will receive)
//         public int ToPartnerId { get; set; }
//         public Partner? ToPartner { get; set; }

//         [Column(TypeName = "decimal(18,2)")]
//         public decimal Amount { get; set; }

//         public DateTime PeriodStart { get; set; }
//         public DateTime PeriodEnd { get; set; }
//     }
// }
