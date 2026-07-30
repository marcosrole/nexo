using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nexo.Shared.Models;

namespace Nexo.Server.Data
{
    public class NexoDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public NexoDbContext(DbContextOptions<NexoDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<Tarifa> Tarifas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(u => u.ClienteId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Proyecto>(entity =>
            {
                entity.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);
                entity.Property(p => p.HorasContratadas).HasPrecision(10, 2);

                entity.HasOne(p => p.Cliente)
                    .WithMany()
                    .HasForeignKey(p => p.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(p => p.ProductorResponsableId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Tarifa)
                    .WithOne()
                    .HasForeignKey<Tarifa>(t => t.ProyectoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Tarifa>(entity =>
            {
                entity.Property(t => t.Modalidad).HasConversion<string>().HasMaxLength(20);
                entity.Property(t => t.Valor).HasPrecision(12, 2);
            });
        }
    }
}
