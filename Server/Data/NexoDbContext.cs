using Microsoft.EntityFrameworkCore;
using Nexo.Shared.Models;

namespace Nexo.Server.Data
{
    public class NexoDbContext : DbContext
    {
        public NexoDbContext(DbContextOptions<NexoDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20);
            });
        }
    }
}
