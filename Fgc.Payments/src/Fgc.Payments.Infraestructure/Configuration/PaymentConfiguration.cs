using Fgc.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fgc.Payments.Infraestructure.Configuration
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedNever();

            builder.Property(p => p.UserId)
                .IsRequired();

            builder.Property(p => p.GameId)
            .IsRequired();

            builder.Property(p => p.Amount)
                .IsRequired();

            builder.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
            .IsRequired();

            builder.Property(p => p.ProcessedAt);
        }
    }
}
