using Fgc.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fgc.Payments.Infraestructure.Persistence
{
    public class FgcPaymentsDbContext(DbContextOptions<FgcPaymentsDbContext> options)
        : DbContext(options)
    {
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(FgcPaymentsDbContext).Assembly);
        }
    }
}
