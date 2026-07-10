using Fgc.Payments.Domain.Enums;

namespace Fgc.Payments.Application.Services
{
    public record PaymentResponse(Guid Id, PaymentStatus Status);
}
