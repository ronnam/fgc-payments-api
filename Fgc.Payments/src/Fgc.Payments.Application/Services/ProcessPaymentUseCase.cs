using Fgc.MessageContracts.Events;
using Fgc.Payments.Application.Interfaces;
using Fgc.Payments.Domain.Entities;
using Fgc.Payments.Domain.Enums;
using MassTransit;

namespace Fgc.Payments.Application.Services
{
    public class ProcessPaymentUseCase (
        IPaymentRepository paymentRepository,
        IPublishEndpoint publishEndpoint)
    {
        public async Task<PaymentResponse> ProcessAsync(OrderPlacedEvent orderEvent)
        {
            var payment = Payment.Create(
                orderEvent.OrderId,
                orderEvent.UserId,
                orderEvent.GameId,
                orderEvent.Price
            );

            await paymentRepository.AddAsync(payment);

            payment.Approve();

            await paymentRepository.UpdateAsync(payment);

            await publishEndpoint.Publish(new PaymentProcessedEvent(
                OrderedId: payment.Id,
                UserId: payment.UserId,
                GameId: payment.GameId,
                Price: payment.Amount,
                Status: payment.Status.ToString(),
                ProcessedAt: payment.ProcessedAt!.Value
                ));

            return new PaymentResponse(payment.Id, PaymentStatus.Approved);
        }
    }
}
