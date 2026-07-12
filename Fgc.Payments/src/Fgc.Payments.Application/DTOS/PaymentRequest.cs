namespace Fgc.Payments.Application.DTOS
{
    public record PaymentRequest(
        Guid OrderId,
        Guid UserId,
        Guid GameId,
        decimal Amount
    );
}
