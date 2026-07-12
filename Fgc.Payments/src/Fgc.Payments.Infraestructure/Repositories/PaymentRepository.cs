using Fgc.Payments.Application.Interfaces;
using Fgc.Payments.Domain.Entities;
using Fgc.Payments.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fgc.Payments.Infraestructure.Repositories;

public class PaymentRepository(PaymentsDbContext context)
    : IPaymentRepository
{
    public async Task AddAsync(Payment payment)
    {
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment payment)
    {
        context.Payments.Update(payment);
        await context.SaveChangesAsync();
    }

    public async Task<Payment?> GetPaymentByIdAsync(Guid id)
    {
        return await context.Payments.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await context.Payments.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId)
    {
        return await context.Payments.FirstOrDefaultAsync(p => p.Id == orderId);
    }
}