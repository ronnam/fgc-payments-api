using Fgc.MessageContracts.Events;
using Fgc.Payments.Application.DTOS;
using Fgc.Payments.Application.Services;

namespace Fgc.Payments.Application.Interfaces
{
    public interface IProcessPaymentUseCase
    {
        Task<PaymentResponse> ProcessAsync(ProcessPaymentCommand command);
    }
}
