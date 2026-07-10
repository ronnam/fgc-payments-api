using Fgc.MessageContracts.Events;
using Fgc.Payments.Application.DTOS;
using Fgc.Payments.Application.Interfaces;
using Fgc.Payments.Domain.Entities;
using Fgc.Payments.Domain.Enums;
using MassTransit;

namespace Fgc.Payments.Application.Services
{
    public class ProcessPaymentUseCase (
        IPaymentRepository paymentRepository,
        IPublishEndpoint publishEndpoint) : IProcessPaymentUseCase
    {
        public async Task<PaymentResponse> ProcessAsync(ProcessPaymentCommand command)
        {
            var payment = Payment.Create(
                command.OrderId,
                command.UserId,
                command.GameId,
                command.Amount
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
