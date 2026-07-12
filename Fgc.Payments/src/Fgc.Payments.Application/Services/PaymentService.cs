using Fgc.MessageContracts.Events;
using Fgc.Payments.Application.DTOS;
using Fgc.Payments.Application.Interfaces;
using Fgc.Payments.Domain.Entities;
using Fgc.Payments.Domain.Exceptions;
using MassTransit;

namespace Fgc.Payments.Application.Services
{
    public class PaymentService (
        IPaymentRepository paymentRepository,
        IPublishEndpoint publishEndpoint) : IPaymentService
    {
        public async Task<PaymentResponse?> GetByIdAsync(Guid id)
        {
            var payment = await paymentRepository.GetByIdAsync(id);

            return payment is null ? null : PaymentResponse.FromPayment(payment);
        }

        public async Task<PaymentResponse> ProcessAsync(PaymentRequest command)
        {
            var existingPayment = await paymentRepository.GetByOrderIdAsync(command.OrderId);
            if(existingPayment is not null)
            {
                throw new PaymentAlreadyProcessedException(
                    existingPayment.Id,
                    existingPayment.Status
                );
            } 

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

            return PaymentResponse.FromPayment(payment);
        }
    }
}
