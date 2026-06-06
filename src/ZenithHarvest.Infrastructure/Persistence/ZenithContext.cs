namespace ZenithHarvest.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using ZenithHarvest.Domain.Entities;

public class ZenithContext : DbContext
{
    public ZenithContext(DbContextOptions<ZenithContext> options) : base(options) { }

    public DbSet<Insurer> Insurers { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Insurer
        modelBuilder.Entity<Insurer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CNPJ).HasMaxLength(18).IsRequired();
            entity.Property(e => e.Nome).HasMaxLength(255).IsRequired();
            entity.Property(e => e.CodigoSUSEP).HasMaxLength(10).IsRequired();
            entity.HasIndex(e => e.CNPJ).IsUnique();
        });

        // Policy — DIP: configurado por FK no Fluent, não por annotation
        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumeroApolice).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Premio).HasPrecision(12, 2);
            entity.Property(e => e.Status).HasMaxLength(20);
            
            // Relacionamento 1:N — Policy para Insurer
            entity.HasOne(p => p.Insurer)
                .WithMany(i => i.Policies)
                .HasForeignKey(p => p.InsurerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Claim — relacionamento 1:N — Claim para Policy
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NDVIAntes).HasPrecision(5, 3);
            entity.Property(e => e.NDVIDepois).HasPrecision(5, 3);
            entity.Property(e => e.ValorSinistro).HasPrecision(12, 2);
            entity.Property(e => e.Evento).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(c => c.Policy)
                .WithMany(p => p.Claims)
                .HasForeignKey(c => c.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(u => u.Insurer)
                .WithMany(i => i.Users)
                .HasForeignKey(u => u.InsurerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
