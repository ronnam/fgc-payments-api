namespace Fgc.Payments.Application.DTOS
{
    public record ProcessPaymentCommand(
        Guid OrderId,
        Guid UserId,
        Guid GameId,
        decimal Amount
    );
}
