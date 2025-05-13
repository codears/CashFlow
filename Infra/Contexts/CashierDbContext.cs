using CashFlow.Infra.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infra.Contexts
{
    public class CashierDbContext : DbContext
    {
        public CashierDbContext(DbContextOptions<CashierDbContext> options)
            : base(options)
        {
        }

        public DbSet<CashPosting> CashPostings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CashPosting>(entity =>
            {
                entity.ToTable("cash_postings");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.Amount)
                    .HasColumnName("amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(e => e.PostingType)
                    .HasColumnName("posting_type")
                    .HasMaxLength(1)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(255);
            });
        }
    }
}

