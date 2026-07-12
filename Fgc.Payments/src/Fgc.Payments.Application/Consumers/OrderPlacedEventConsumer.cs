using Fgc.MessageContracts.Events;
using Fgc.Payments.Application.DTOS;
using Fgc.Payments.Application.Interfaces;
using MassTransit;

namespace Fgc.Payments.Application.Consumers
{
    public class OrderPlacedEventConsumer(
        IPaymentService processPaymentUseCase)
        : IConsumer<OrderPlacedEvent>
    {
        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var command = new PaymentRequest(
                context.Message.OrderId,
                context.Message.UserId,
                context.Message.GameId,
                context.Message.Price
            );
            await processPaymentUseCase.ProcessAsync(command);
        }
    }
}
