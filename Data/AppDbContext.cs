using Microsoft.EntityFrameworkCore;
using FinanceSharingApp.Models;

namespace FinanceSharingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Partner> Partners { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Revenue> Revenues { get; set; }
        public DbSet<ExpenseShare> ExpenseShares { get; set; }
        public DbSet<RevenueShare> RevenueShares { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Partner)
                .WithMany(p => p.Expenses)
                .HasForeignKey(e => e.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Revenue>()
                .HasOne(r => r.Partner)
                .WithMany(p => p.Revenues)
                .HasForeignKey(r => r.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExpenseShare>()
                .HasOne(es => es.Expense)
                .WithMany(e => e.ExpenseShares)
                .HasForeignKey(es => es.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExpenseShare>()
                .HasOne(es => es.Partner)
                .WithMany()
                .HasForeignKey(es => es.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RevenueShare>()
                .HasOne(rs => rs.Revenue)
                .WithMany(r => r.RevenueShares)
                .HasForeignKey(rs => rs.RevenueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RevenueShare>()
                .HasOne(rs => rs.Partner)
                .WithMany()
                .HasForeignKey(rs => rs.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
