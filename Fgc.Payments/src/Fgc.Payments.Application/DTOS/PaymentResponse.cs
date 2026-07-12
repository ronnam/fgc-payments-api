using Fgc.Payments.Domain.Entities;

namespace Fgc.Payments.Application.DTOS
{
    public record PaymentResponse(
        Guid Id,
        Guid UserId,
        Guid GameId,
        decimal Amount,
        string Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt
    )
    {
        public static PaymentResponse FromPayment(Payment payment) => new(
            payment.Id,
            payment.UserId,
            payment.GameId,
            payment.Amount,
            payment.Status.ToString(),
            payment.CreatedAt,
            payment.ProcessedAt
        );
    }
}