using Fgc.MessageContracts.Events;
using Fgc.Payments.Domain.Entities;
using Fgc.Payments.Infraestructure.Persistence;
using MassTransit;

namespace Fgc.Payments.Api.Consumers
{
    public class OrderPlacedEventConsumer(
        PaymentsDbContext dbContext,
        ILogger<OrderPlacedEventConsumer> logger) : IConsumer<OrderPlacedEvent>
    {
        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var payment = Payment.Create(
                context.Message.OrderId,
                context.Message.UserId,
                context.Message.GameId,
                context.Message.Price
            );

            payment.Approve();

            await dbContext.Payments.AddAsync( payment );
            await dbContext.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation(
                "Payment processed: {PaymentId}, User: {UserId}, Game: {GameId}, Amount: {Amount}",
                payment.Id, payment.UserId, payment.GameId, payment.Amount
            );
        }
    }
}
