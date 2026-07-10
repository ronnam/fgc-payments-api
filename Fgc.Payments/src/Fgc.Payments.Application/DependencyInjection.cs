using Fgc.Payments.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fgc.Payments.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<ProcessPaymentUseCase>();
            return services;
        }
    }
}
