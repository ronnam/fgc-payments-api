using Fgc.Payments.Application.Interfaces;
using Fgc.Payments.Infraestructure.Persistence;
using Fgc.Payments.Infraestructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fgc.Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PaymentsDb");

        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        return services;
    }
}