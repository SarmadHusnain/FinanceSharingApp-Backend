using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FinanceSharingApp.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Partner
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!; //  Password (hashed)

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Partner"; //  "Admin" or "Partner"

        public DateTime DateJoined { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public DateTime? RemoveDate { get; set; }

        // Navigation Collections
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<Revenue> Revenues { get; set; } = new List<Revenue>();
    }
}
