using Fgc.Payments.Domain.Entities;

namespace Fgc.Payments.Application.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetPaymentByIdAsync(Guid id);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
    }
}
