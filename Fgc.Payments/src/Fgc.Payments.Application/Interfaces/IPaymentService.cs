using Fgc.Payments.Application.DTOS;

namespace Fgc.Payments.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessAsync(PaymentRequest command);
        Task<PaymentResponse?> GetByIdAsync(Guid id);
    }
}
