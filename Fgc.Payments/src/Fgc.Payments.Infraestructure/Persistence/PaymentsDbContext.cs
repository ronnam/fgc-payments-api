using Fgc.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fgc.Payments.Infraestructure.Persistence
{
    public class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
        : DbContext(options)
    {
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(PaymentsDbContext).Assembly);
        }
    }
}
