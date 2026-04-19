using Microsoft.EntityFrameworkCore;
using Corvux.Models;

namespace Corvux.Data;

/// <summary>
/// DbContext de EF Core. Tabla Transactions con configuración de índices y restricciones.
/// </summary>
public class ComplianceDbContext : DbContext
{
    public ComplianceDbContext(DbContextOptions<ComplianceDbContext> options) : base(options) { }

    public DbSet<TransactionRecord> Transactions => Set<TransactionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionRecord>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TransactionId).IsUnique();
            entity.Property(e => e.TransactionId).HasMaxLength(40).IsRequired();
            entity.Property(e => e.AmountUsd).HasPrecision(18, 2);
            entity.Property(e => e.OriginCountry).HasMaxLength(80).IsRequired();
            entity.Property(e => e.DestinationCountry).HasMaxLength(80).IsRequired();
        });
    }
}
